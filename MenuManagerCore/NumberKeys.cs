using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using MenuManager;
using Microsoft.Extensions.Logging;

namespace MenuManagerCore;

// css_1 ... css_0 from the player's number-key binds. One place decides who gets a key:
// the number keys check, then CSGO Menu, then the digits panorama. Plugins see every key through
// IMenuApi.NumberKeyPressed (or their own AddCommandListener on css_N).
internal static class NumberKeys
{
    // spec_usenumberkeys_nobinds 0 lets the number keys run their binds while dead or spectating.
    // Tested in game: the space after ";" inside the bind is required ("css_1; slot1"). Key 0 is slot10.
    // The server cannot set the cvar or the binds (CS2: "missing required FCVAR flag"), the player pastes this once.
    internal const string BindLine =
        "spec_usenumberkeys_nobinds 0;bind \"1\" \"css_1; slot1\";bind \"2\" \"css_2; slot2\";bind \"3\" \"css_3; slot3\";bind \"4\" \"css_4; slot4\";bind \"5\" \"css_5; slot5\";bind \"6\" \"css_6; slot6\";bind \"7\" \"css_7; slot7\";bind \"8\" \"css_8; slot8\";bind \"9\" \"css_9; slot9\";bind \"0\" \"css_0; slot10\"";

    private static readonly int[] Order = [1, 2, 3, 4, 5, 6, 7, 8, 9, 0];
    private static readonly Check?[] Checks = new Check?[64];

    internal static event Action<CCSPlayerController, int>? Pressed;

    internal static void Register(MenuManagerCore plugin)
    {
        for (int key = 0; key <= 9; key++)
        {
            int digit = key;
            plugin.AddCommand($"css_{digit}", "Menu number key", (player, _) => OnKey(player, digit));
        }

        plugin.AddCommandListener("menuselect", OnMenuSelect);
        plugin.AddTimer(1.0f, CheckTimeouts, TimerFlags.REPEAT);
    }

    internal static void Reset()
    {
        Array.Clear(Checks);
    }

    internal static void Forget(int slot)
    {
        if (slot is >= 0 and < 64)
        {
            Checks[slot] = null;
        }
    }

    internal static bool IsChecking(CCSPlayerController player)
    {
        return player is { IsValid: true, Slot: >= 0 and < 64 } && Checks[player.Slot] != null;
    }

    private static void OnKey(CCSPlayerController? player, int digit)
    {
        if (player == null || !Misc.IsValidPlayer(player) || player.Slot is < 0 or >= 64)
        {
            return;
        }

        Raise(player, digit);
        Consume(player, digit);
    }

    private static HookResult OnMenuSelect(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !Misc.IsValidPlayer(player) || player.Slot is < 0 or >= 64)
        {
            return HookResult.Continue;
        }

        string arg = info.ArgCount > 1 ? info.GetArg(1) : "";
        if (!int.TryParse(arg, out int key) || key is < 0 or > 10)
        {
            return HookResult.Continue;
        }

        return Consume(player, key % 10) ? HookResult.Handled : HookResult.Continue;
    }

    private static bool Consume(CCSPlayerController player, int digit)
    {
        if (Checks[player.Slot] is { } check)
        {
            OnCheckKey(player, check, digit);
            return true;
        }

        if (CsgoMenu.IsOpen(player))
        {
            CsgoMenu.Press(player, digit);
            return true;
        }

        return PanoramaHud.OnDigit(player, digit);
    }

    private static void Raise(CCSPlayerController player, int digit)
    {
        Action<CCSPlayerController, int>? handlers = Pressed;
        if (handlers == null)
        {
            return;
        }

        foreach (Delegate handler in handlers.GetInvocationList())
        {
            try
            {
                ((Action<CCSPlayerController, int>)handler)(player, digit);
            }
            catch (Exception e)
            {
                Control.GetPlugin()?.Logger.LogError(e, "NumberKeyPressed handler failed");
            }
        }
    }

    // ---------------- number keys check ----------------

    // The CSGO-style panel shows 1..0; each key lights up when the server gets its css_N.
    // Before all ten: a key that is already lit works as 8 = back, 0 = exit.
    // After all ten: 1 = done (turns the target menu on), 2 = again, 8 = back, 0 = exit.
    // No key for NumberKeysTestTimeout seconds: back (or exit).
    internal static void Start(CCSPlayerController player, MenuType? target,
        Action<CCSPlayerController, bool>? done, Action<CCSPlayerController>? back)
    {
        if (!Misc.IsValidPlayer(player) || player.Slot is < 0 or >= 64)
        {
            return;
        }

        Abort(player);
        CounterStrikeSharp.API.Modules.Menu.MenuManager.CloseActiveMenu(player);
        Control.CloseMenu(player);
        PanoramaHud.CloseMenu(player);
        CsgoMenu.Close(player);
        PaintRegistry.Close(player);

        Check check = new() { Target = target, Done = done, Back = back, LastKey = Server.CurrentTime };
        Checks[player.Slot] = check;
        PrintBinds(player);

        MenuManagerCore? plugin = Control.GetPlugin();
        if (plugin == null)
        {
            return;
        }

        // The say command and the HUD update must not share a frame (see PanoramaHud.Show).
        plugin.AddTimer(Math.Max(0.01f, plugin.Config.HudOpenDelay), () =>
        {
            if (Misc.IsValidPlayer(player) && ReferenceEquals(Checks[player.Slot], check))
            {
                Draw(player, check);
            }
        });
    }

    // Another menu opens, or the API closes the menus: the check ends without a result.
    internal static void Abort(CCSPlayerController player)
    {
        if (!player.IsValid || player.Slot is < 0 or >= 64 || Checks[player.Slot] is not { } check)
        {
            return;
        }

        Checks[player.Slot] = null;
        CsgoMenu.HidePanel(player);
        Run(() => check.Done?.Invoke(player, false));
    }

    private static void OnCheckKey(CCSPlayerController player, Check check, int digit)
    {
        MenuManagerCore? plugin = Control.GetPlugin();
        check.LastKey = Server.CurrentTime;
        if (!check.AllDone)
        {
            if (!check.Keys[digit])
            {
                check.Keys[digit] = true;
                Control.PlaySound(player, (check.AllDone ? plugin?.Config.SoundClick : plugin?.Config.SoundScroll) ?? "");
                Draw(player, check);
                return;
            }

            if (digit == 8 && check.Back != null)
            {
                GoBack(player, check);
            }
            else if (digit == 0)
            {
                Exit(player, check);
            }

            return;
        }

        switch (digit)
        {
            case 1:
                Finish(player, check);
                return;
            case 2:
                Array.Clear(check.Keys);
                Control.PlaySound(player, plugin?.Config.SoundClick ?? "");
                Draw(player, check);
                return;
            case 8:
                if (check.Back != null)
                {
                    GoBack(player, check);
                }
                else
                {
                    Control.PlaySound(player, plugin?.Config.SoundDisabled ?? "");
                }

                return;
            case 0:
                Exit(player, check);
                return;
        }
    }

    private static void Finish(CCSPlayerController player, Check check)
    {
        Control.PlaySound(player, Control.GetPlugin()?.Config.SoundClick ?? "");
        Checks[player.Slot] = null;
        CsgoMenu.HidePanel(player);
        Misc.SetNumberKeysChecked(player, true);
        if (check.Target is { } type)
        {
            Misc.SelectPlayerMenu(player, type);
        }

        Run(() => check.Done?.Invoke(player, true));
    }

    private static void Exit(CCSPlayerController player, Check check)
    {
        Control.PlaySound(player, Control.GetPlugin()?.Config.SoundExit ?? "");
        Checks[player.Slot] = null;
        CsgoMenu.HidePanel(player);
        Run(() => check.Done?.Invoke(player, false));
    }

    private static void GoBack(CCSPlayerController player, Check check)
    {
        Control.PlaySound(player, Control.GetPlugin()?.Config.SoundBack ?? "");
        Checks[player.Slot] = null;
        CsgoMenu.HidePanel(player);
        Run(() => check.Done?.Invoke(player, false));
        if (check.Back is { } back)
        {
            Run(() => back(player));
        }
    }

    private static void CheckTimeouts()
    {
        MenuManagerCore? plugin = Control.GetPlugin();
        if (plugin == null)
        {
            return;
        }

        float limit = Math.Max(10f, plugin.Config.NumberKeysTestTimeout);
        for (int slot = 0; slot < Checks.Length; slot++)
        {
            Check? check = Checks[slot];
            if (check == null)
            {
                continue;
            }

            CCSPlayerController? player = Utilities.GetPlayerFromSlot(slot);
            if (player == null || !Misc.IsValidPlayer(player))
            {
                Checks[slot] = null;
                continue;
            }

            if (Server.CurrentTime - check.LastKey < limit)
            {
                continue;
            }

            if (check.Back != null)
            {
                GoBack(player, check);
            }
            else
            {
                Exit(player, check);
            }
        }
    }

    private static void Draw(CCSPlayerController player, Check check)
    {
        MenuManagerCore? plugin = Control.GetPlugin();
        if (plugin == null)
        {
            return;
        }

        List<CsgoMenu.Line> lines = [];
        if (check.AllDone)
        {
            string enable = check.Target is { } type
                ? T("menumanager.keys_enable", Misc.GetMenuTypeName(type))
                : T("menumanager.keys_ok");
            lines.Add(new CsgoMenu.Line("1. " + enable, true));
            lines.Add(new CsgoMenu.Line("2. " + T("menumanager.keys_again"), true));
            lines.Add(new CsgoMenu.Line("", false));
            lines.Add(new CsgoMenu.Line(T("menumanager.keys_done"), false));
        }
        else
        {
            int count = check.Keys.Count(k => k);
            string left = string.Join(" ", Order.Where(d => !check.Keys[d]));
            int limit = (int)Math.Max(10f, plugin.Config.NumberKeysTestTimeout);
            lines.Add(new CsgoMenu.Line(T("menumanager.keys_press"), false));
            lines.Add(new CsgoMenu.Line(T("menumanager.keys_progress", count, left), false));
            lines.Add(new CsgoMenu.Line("", false));
            lines.Add(new CsgoMenu.Line(T("menumanager.keys_help1"), false));
            lines.Add(new CsgoMenu.Line(T("menumanager.keys_help2"), false));
            lines.Add(new CsgoMenu.Line(T(check.Back != null ? "menumanager.keys_help3" : "menumanager.keys_help3_exit"), false));
            lines.Add(new CsgoMenu.Line(T("menumanager.keys_help4", limit), false));
        }

        string? back = check.Back != null ? "8. " + T("menumanager.csgo_back") : null;
        CsgoMenu.DrawCheck(player, T("menumanager.keys_test"), check.Keys, lines, back,
            "0. " + T("menumanager.csgo_exit"));
    }

    // ---------------- binds ----------------

    internal static void PrintBinds(CCSPlayerController player)
    {
        string how = T("menumanager.keys_chat");
        Server.NextFrame(() =>
        {
            if (!player.IsValid)
            {
                return;
            }

            player.PrintToChat($" {ChatColors.Red}[MENU] {ChatColors.Green}{how}");
            player.PrintToChat($" {BindLine}");
            player.PrintToConsole(how);
            player.PrintToConsole(BindLine);
        });
    }

    private static string T(string key, params object[] args)
    {
        MenuManagerCore? plugin = Control.GetPlugin();
        if (plugin == null)
        {
            return key;
        }

        return args.Length == 0 ? plugin.Localizer[key].Value : plugin.Localizer[key, args].Value;
    }

    private static void Run(Action action)
    {
        try
        {
            action();
        }
        catch (Exception e)
        {
            Control.GetPlugin()?.Logger.LogError(e, "Number keys callback failed");
        }
    }

    private sealed class Check
    {
        public readonly bool[] Keys = new bool[10];
        public MenuType? Target;
        public Action<CCSPlayerController, bool>? Done;
        public Action<CCSPlayerController>? Back;
        public float LastKey;
        public bool AllDone => Keys.All(k => k);
    }
}
