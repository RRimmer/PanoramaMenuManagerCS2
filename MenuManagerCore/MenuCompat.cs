using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using MenuManager;
using Microsoft.Extensions.Logging;
using GameTimer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace MenuManagerCore;

/// <summary>
/// Menus of plugins built against CS2MenuManager (schwarper), e.g. cs2-store.
/// The CS2MenuManager.dll in shared/ only forwards here (capability "menu:nfcore:compat").
/// One session per slot remembers which of those menus is on screen.
/// </summary>
internal sealed class MenuCompatApi(CMenuApi menus) : IMenuCompatApi
{
    private sealed class Session(CompatMenu menu, ulong steamId, long serial)
    {
        public CompatMenu Menu { get; } = menu;
        public ulong SteamId { get; } = steamId;
        public long Serial { get; } = serial;
        public GameTimer? Timer { get; set; }
    }

    private readonly Dictionary<int, Session> _sessions = [];
    private long _serial;

    public void Show(CCSPlayerController player, CompatMenu menu)
    {
        if (player == null || !player.IsValid || player.IsBot)
        {
            return;
        }

        List<CompatMenuItem> items = menu.Items.ToList();
        if (items.Count == 0)
        {
            player.PrintToChat(EmptyText(player));
            return;
        }

        int slot = player.Slot;
        long serial = ++_serial;
        Drop(slot);

        // PMM must not hide the panel before the callback: the callback usually opens
        // the next menu. What happens after the click is decided per item in Select.
        MenuInstance pmm = new(menu.Title, menu.Back)
        {
            ExitButton = menu.ExitButton,
            PostSelectAction = PostSelectAction.Nothing
        };

        foreach (CompatMenuItem item in items)
        {
            _ = pmm.AddMenuOption(item.Text ?? string.Empty, (p, _) => Select(menu, item, p, serial), item.Disabled);
        }

        Session session = new(menu, player.SteamID, serial);
        _sessions[slot] = session;

        if (menu.Time > 0)
        {
            session.Timer = Control.GetPlugin()?.AddTimer(menu.Time, () =>
            {
                if (IsCurrent(slot, serial))
                {
                    Close(player);
                }
            });
        }

        pmm.Open(player);
    }

    public void Close(CCSPlayerController player)
    {
        if (!TryGet(player, out _))
        {
            return;
        }

        Drop(player.Slot);
        menus.CloseMenu(player);
    }

    public object? GetActive(CCSPlayerController player)
    {
        if (!TryGet(player, out Session? session))
        {
            return null;
        }

        // The player may have closed the panel with the PMM exit button.
        return menus.HasOpenedMenu(player) ? session.Menu.Owner : null;
    }

    public void KeyPress(CCSPlayerController player, int key)
    {
        if (!TryGet(player, out Session? session))
        {
            return;
        }

        CompatMenu menu = session.Menu;
        if (key == 0)
        {
            if (menu.ExitButton)
            {
                Close(player);
            }

            return;
        }

        if (key == 8 && menu.Back != null)
        {
            menu.Back(player);
            return;
        }

        int index = key - 1;
        if (index < 0 || index >= menu.Items.Count || menu.Items[index].Disabled)
        {
            return;
        }

        Select(menu, menu.Items[index], player, session.Serial);
    }

    internal void Forget(int slot)
    {
        Drop(slot);
    }

    private void Select(CompatMenu menu, CompatMenuItem item, CCSPlayerController? player, long serial)
    {
        if (player == null || !player.IsValid)
        {
            return;
        }

        CompatPostSelect after = CompatPostSelect.Close;
        try
        {
            if (item.OnSelect != null)
            {
                after = item.OnSelect(player);
            }
        }
        catch (Exception e)
        {
            Control.GetPlugin()?.Logger.LogError(e, "CS2MenuManager item \"{Item}\" in \"{Menu}\" failed",
                item.Text, menu.Title);
        }

        // The callback opened another menu (or closed this one): leave it alone.
        if (!player.IsValid || !IsCurrent(player.Slot, serial))
        {
            return;
        }

        switch (after)
        {
            case CompatPostSelect.Close:
                Close(player);
                break;
            case CompatPostSelect.Reset:
                menu.Reopen?.Invoke(player);
                break;
        }
    }

    private bool IsCurrent(int slot, long serial)
    {
        return _sessions.TryGetValue(slot, out Session? session) && session.Serial == serial;
    }

    private bool TryGet(CCSPlayerController? player,
        [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Session? session)
    {
        session = null;
        if (player == null || !player.IsValid || !_sessions.TryGetValue(player.Slot, out Session? found))
        {
            return false;
        }

        // The slot now belongs to another player.
        if (found.SteamId != 0 && player.SteamID != 0 && found.SteamId != player.SteamID)
        {
            Drop(player.Slot);
            return false;
        }

        session = found;
        return true;
    }

    private void Drop(int slot)
    {
        if (!_sessions.Remove(slot, out Session? old))
        {
            return;
        }

        try
        {
            old.Timer?.Kill();
        }
        catch (Exception)
        {
            // Timer already gone.
        }
    }

    private static string EmptyText(CCSPlayerController player)
    {
        string lang = player.GetLanguage().TwoLetterISOLanguageName;
        return lang is "ru" or "uk"
            ? $" {ChatColors.Red}[Меню]{ChatColors.Default} В этом меню нет пунктов."
            : $" {ChatColors.Red}[Menu]{ChatColors.Default} This menu has no items.";
    }
}

/// <summary>
/// Writes the embedded CS2MenuManager.dll to shared/CS2MenuManager and resolves it for plugins
/// that load after the core. Config: "CS2MenuManagerCompat".
/// </summary>
internal static class Cs2MenuManagerDeploy
{
    private const string Resource = "CS2MenuManager.dll";

    // Name of a class inside our CS2MenuManager.dll. schwarper's build does not have it.
    private const string Marker = "PmmCompatBridge";

    private static Func<AssemblyLoadContext, AssemblyName, Assembly?>? _resolver;

    internal static void Install(MenuManagerCore plugin)
    {
        ILogger log = plugin.Logger;
        string dir = Path.GetFullPath(Path.Combine(plugin.ModuleDirectory, "..", "..", "shared", "CS2MenuManager"));
        string target = Path.Combine(dir, "CS2MenuManager.dll");

        try
        {
            byte[]? ours = ReadResource();
            if (ours == null)
            {
                log.LogWarning("CS2MenuManager compat: embedded {Resource} is missing, nothing written", Resource);
                return;
            }

            byte[]? current = File.Exists(target) ? File.ReadAllBytes(target) : null;
            if (current == null || !current.AsSpan().SequenceEqual(ours))
            {
                _ = Directory.CreateDirectory(dir);
                if (current != null && !Contains(current, Marker))
                {
                    File.Copy(target, target + ".original", true);
                    log.LogWarning("CS2MenuManager compat: replaced another CS2MenuManager.dll, the old file is {Backup}",
                        target + ".original");
                }

                string tmp = target + ".tmp";
                File.WriteAllBytes(tmp, ours);
                File.Move(tmp, target, true);
                log.LogInformation(
                    "CS2MenuManager compat written to {Path}. If a plugin failed with CS2MenuManager not found before this, restart the server once.",
                    target);
            }
        }
        catch (Exception e)
        {
            log.LogError(e, "CS2MenuManager compat: cannot write {Path}", target);
        }

        // CSS scans shared/ once, often before this file exists. Plugins that load after
        // the core still find it through this handler.
        Uninstall();
        _resolver = (context, name) =>
            name.Name == "CS2MenuManager" && File.Exists(target) ? context.LoadFromAssemblyPath(target) : null;
        AssemblyLoadContext.Default.Resolving += _resolver;
    }

    internal static void Uninstall()
    {
        if (_resolver == null)
        {
            return;
        }

        AssemblyLoadContext.Default.Resolving -= _resolver;
        _resolver = null;
    }

    private static byte[]? ReadResource()
    {
        using Stream? stream = typeof(Cs2MenuManagerDeploy).Assembly.GetManifestResourceStream(Resource);
        if (stream == null)
        {
            return null;
        }

        using MemoryStream memory = new();
        stream.CopyTo(memory);
        return memory.ToArray();
    }

    private static bool Contains(byte[] data, string text)
    {
        return data.AsSpan().IndexOf(Encoding.ASCII.GetBytes(text)) >= 0;
    }
}
