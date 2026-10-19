using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CS2MenuManager.API.Class;
using CS2MenuManager.API.Enum;
using CS2MenuManager.API.Interface;
using CompatApi = global::MenuManager.IMenuCompatApi;
using CompatItem = global::MenuManager.CompatMenuItem;
using CompatMenu = global::MenuManager.CompatMenu;
using CompatPost = global::MenuManager.CompatPostSelect;

namespace CS2MenuManager.Pmm;

/// <summary>
/// Hands a CS2MenuManager menu to MenuManagerCore (capability "menu:nfcore:compat").
/// No menu logic here: sessions, paging, timers and closing live in the core.
/// </summary>
internal static class PmmCompatBridge
{
    private static readonly PluginCapability<CompatApi?> Capability = new("menu:nfcore:compat");
    private static bool _warned;

    private static CompatApi? Api
    {
        get
        {
            try
            {
                return Capability.Get();
            }
            catch (Exception)
            {
                // KeyNotFoundException until MenuManagerCore registers the capability.
                return null;
            }
        }
    }

    internal static void Show(IMenu menu, CCSPlayerController? player, int time)
    {
        if (player == null || !player.IsValid || player.IsBot)
        {
            return;
        }

        CompatApi? api = Api;
        if (api == null)
        {
            if (!_warned)
            {
                _warned = true;
                Console.WriteLine("[CS2MenuManager] menu:nfcore:compat not found. MenuManagerCore 1.2.04+ must be loaded. " +
                                  $"Menu \"{menu.Title}\" was not opened.");
            }

            return;
        }

        CompatMenu data = new(menu)
        {
            Title = menu.Title,
            ExitButton = menu.ExitButton,
            Time = time
        };

        if (menu.PrevMenu is { } prev)
        {
            data.Back = p => prev.Display(p, prev.MenuTime);
        }

        data.Reopen = p => menu.Display(p, menu.MenuTime);

        foreach (ItemOption item in menu.ItemOptions.ToList())
        {
            data.Items.Add(new CompatItem(item.Text, item.DisableOption != DisableOption.None, p =>
            {
                item.OnSelect?.Invoke(p, item);
                return item.PostSelectAction switch
                {
                    PostSelectAction.Reset => CompatPost.Reset,
                    PostSelectAction.Nothing => CompatPost.Nothing,
                    _ => CompatPost.Close
                };
            }));
        }

        api.Show(player, data);
    }

    internal static void Close(CCSPlayerController player)
    {
        Api?.Close(player);
    }

    internal static IMenuInstance? GetActive(CCSPlayerController player)
    {
        return Api?.GetActive(player) is IMenu menu ? new PmmMenuInstance(player, menu) : null;
    }

    internal static void KeyPress(CCSPlayerController player, int key)
    {
        Api?.KeyPress(player, key);
    }
}
