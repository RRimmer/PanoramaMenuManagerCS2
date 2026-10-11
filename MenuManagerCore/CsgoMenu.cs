using System.Text;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Extensions;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Timers;
using MenuManager;
using Microsoft.Extensions.Logging;

namespace MenuManagerCore;

internal static class CsgoMenu
{
    private const int PageSize = 7;
    private const string ItemColor = "#e39b2b";
    private const string TitleColor = "#f2c14d";
    private const string DisabledColor = "#8d8d8d";

    private static readonly State?[] Slots = new State?[64];

    internal static void Register(MenuManagerCore plugin)
    {
        for (int key = 0; key <= 9; key++)
        {
            int digit = key;
            plugin.AddCommand($"css_{digit}", "CSGO menu select", (player, _) => OnDigit(player, digit));
        }

        plugin.AddCommandListener("menuselect", OnMenuSelect);
        plugin.AddTimer(1.0f, Refresh, TimerFlags.REPEAT);
    }

    internal static void Reset()
    {
        Array.Clear(Slots);
    }

    internal static bool IsOpen(CCSPlayerController player)
    {
        int slot = player.Slot;
        return slot is >= 0 and < 64 && Slots[slot] != null;
    }

    internal static void Close(CCSPlayerController? player)
    {
        if (player == null || !player.IsValid)
        {
            return;
        }

        int slot = player.Slot;
        if (slot is < 0 or >= 64 || Slots[slot] == null)
        {
            return;
        }

        Slots[slot] = null;
        player.PrintToCenterHtml(" ");
        if (PanoramaHud.ExistingLayout() is { } layout)
        {
            layout.SetHasClassForPlayer(player, "utils-csgo", "csgo-hidden", true);
        }
    }

    internal static void Show(CCSPlayerController player, MenuInstance menu)
    {
        if (!Misc.IsValidPlayer(player))
        {
            return;
        }

        PanoramaHud.CloseMenu(player);
        Control.CloseMenu(player);
        CounterStrikeSharp.API.Modules.Menu.MenuManager.CloseActiveMenu(player);

        int slot = player.Slot;
        if (slot is < 0 or >= 64)
        {
            return;
        }

        State state = new(menu) { Opened = Server.CurrentTime };
        Slots[slot] = state;
        Control.GetPlugin()?.AddTimer(Math.Max(0.01f, Control.GetPlugin()?.Config.HudOpenDelay ?? 0.2f), () =>
        {
            if (Misc.IsValidPlayer(player) && ReferenceEquals(Slots[player.Slot], state))
            {
                Draw(player);
            }
        });
    }

    private static HookResult OnMenuSelect(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !IsOpen(player))
        {
            return HookResult.Continue;
        }

        string arg = info.ArgCount > 1 ? info.GetArg(1) : "";
        if (!int.TryParse(arg, out int key))
        {
            return HookResult.Continue;
        }

        if (key == 10)
        {
            key = 0;
        }

        OnDigit(player, key);
        return HookResult.Handled;
    }

    private static void OnDigit(CCSPlayerController? player, int key)
    {
        if (player == null || !Misc.IsValidPlayer(player) || !IsOpen(player))
        {
            return;
        }

        Press(player, key);
    }

    private static void Refresh()
    {
        MenuManagerCore? plugin = Control.GetPlugin();
        float now = Server.CurrentTime;
        float limit = plugin?.Config.MenuTime ?? 120f;
        for (int slot = 0; slot < Slots.Length; slot++)
        {
            State? state = Slots[slot];
            if (state == null)
            {
                continue;
            }

            CCSPlayerController? player = Utilities.GetPlayerFromSlot(slot);
            if (player == null || !Misc.IsValidPlayer(player))
            {
                Slots[slot] = null;
                continue;
            }

            if (now - state.Opened > limit)
            {
                Control.PlaySound(player, plugin?.Config.SoundExit ?? "");
                Close(player);
            }
        }
    }

    private static void Press(CCSPlayerController player, int key)
    {
        State? state = Slots[player.Slot];
        if (state == null)
        {
            return;
        }

        state.Opened = Server.CurrentTime;
        if (key == 0)
        {
            Control.PlaySound(player, Control.GetPlugin()?.Config.SoundExit ?? "");
            Close(player);
            return;
        }

        if (key == 8)
        {
            Back(player, state);
            return;
        }

        if (key == 9)
        {
            Next(player, state);
            return;
        }

        if (key is < 1 or > 7)
        {
            return;
        }

        if (state.Picker >= 0)
        {
            PickChoice(player, state, state.PickerPage * PageSize + (key - 1));
            return;
        }

        Choose(player, state, state.Page * PageSize + (key - 1));
    }

    private static void Back(CCSPlayerController player, State state)
    {
        if (state.Picker >= 0)
        {
            Control.PlaySound(player, Control.GetPlugin()?.Config.SoundBack ?? "");
            state.Picker = -1;
            Draw(player);
            return;
        }

        if (state.Page > 0)
        {
            Control.PlaySound(player, Control.GetPlugin()?.Config.SoundBack ?? "");
            state.Page--;
            Draw(player);
            return;
        }

        if (state.Menu.BackAction == null)
        {
            Control.PlaySound(player, Control.GetPlugin()?.Config.SoundDisabled ?? "");
            return;
        }

        Control.PlaySound(player, Control.GetPlugin()?.Config.SoundBack ?? "");
        MenuInstance menu = state.Menu;
        Close(player);
        RunCallback(() => menu.BackAction(player));
    }

    private static bool CanGoBack(State state)
    {
        return state.Picker >= 0 || state.Page > 0 || state.Menu.BackAction != null;
    }

    private static void Next(CCSPlayerController player, State state)
    {
        int count = state.Picker >= 0 ? ChoiceCount(state) : state.Menu.MenuOptions.Count;
        int page = state.Picker >= 0 ? state.PickerPage : state.Page;
        if (page >= PageCount(count) - 1)
        {
            return;
        }

        Control.PlaySound(player, Control.GetPlugin()?.Config.SoundScroll ?? "");
        if (state.Picker >= 0)
        {
            state.PickerPage++;
        }
        else
        {
            state.Page++;
        }

        Draw(player);
    }

    private static void Choose(CCSPlayerController player, State state, int index)
    {
        MenuInstance menu = state.Menu;
        if (index < 0 || index >= menu.MenuOptions.Count)
        {
            return;
        }

        ChatMenuOption option = menu.MenuOptions[index];
        MenuManagerCore? plugin = Control.GetPlugin();
        if (option.Disabled)
        {
            Control.PlaySound(player, plugin?.Config.SoundDisabled ?? "");
            return;
        }

        if (MenuOptionKind.TryGetSelect(option, out MenuOptionKind.SelectState select) && select.Choices.Length > 0)
        {
            Control.PlaySound(player, plugin?.Config.SoundClick ?? "");
            state.Picker = index;
            int current = Array.FindIndex(select.Choices,
                choice => choice.Equals(select.Value, StringComparison.OrdinalIgnoreCase));
            state.PickerPage = Math.Max(0, current) / PageSize;
            Draw(player);
            return;
        }

        Control.PlaySound(player, plugin?.Config.SoundClick ?? "");
        if (menu.PostSelectAction != PostSelectAction.Nothing)
        {
            Close(player);
        }

        RunCallback(() => option.OnSelect(player, option));
        if (menu.PostSelectAction == PostSelectAction.Reset && menu.ResetAction != null && !IsOpen(player))
        {
            Server.NextFrame(() =>
            {
                if (!IsOpen(player))
                {
                    menu.ResetAction(player);
                }
            });
        }
        else if (IsOpen(player) && ReferenceEquals(Slots[player.Slot], state))
        {
            Draw(player);
        }
    }

    private static void PickChoice(CCSPlayerController player, State state, int choice)
    {
        if (state.Picker < 0 || state.Picker >= state.Menu.MenuOptions.Count)
        {
            state.Picker = -1;
            Draw(player);
            return;
        }

        ChatMenuOption option = state.Menu.MenuOptions[state.Picker];
        if (!MenuOptionKind.TryGetSelect(option, out MenuOptionKind.SelectState select) ||
            choice < 0 || choice >= select.Choices.Length)
        {
            return;
        }

        Control.PlaySound(player, Control.GetPlugin()?.Config.SoundClick ?? "");
        int picked = choice;
        state.Picker = -1;
        if (state.Menu.PostSelectAction != PostSelectAction.Nothing)
        {
            Close(player);
        }

        RunCallback(() => select.OnSelect?.Invoke(player, option, picked));
        if (IsOpen(player) && ReferenceEquals(Slots[player.Slot], state))
        {
            Draw(player);
        }
    }

    private static void Draw(CCSPlayerController player)
    {
        State? state = player.Slot is >= 0 and < 64 ? Slots[player.Slot] : null;
        if (state == null)
        {
            return;
        }

        state.LastDraw = Server.CurrentTime;
        MenuManagerCore? plugin = Control.GetPlugin();
        string back = plugin?.Localizer["menumanager.csgo_back"] ?? "Back";
        string next = plugin?.Localizer["menumanager.csgo_next"] ?? "Next";
        string exit = plugin?.Localizer["menumanager.csgo_exit"] ?? "Exit";

        string title;
        IReadOnlyList<string> lines;
        int page;
        int count;
        if (state.Picker >= 0 &&
            state.Picker < state.Menu.MenuOptions.Count &&
            MenuOptionKind.TryGetSelect(state.Menu.MenuOptions[state.Picker], out MenuOptionKind.SelectState select))
        {
            title = Plain(state.Menu.MenuOptions[state.Picker].Text);
            lines = select.Choices
                .Select(choice => MenuOptionKind.WithState(choice, choice.Equals(select.Value, StringComparison.OrdinalIgnoreCase)))
                .ToArray();
            page = state.PickerPage;
            count = select.Choices.Length;
        }
        else
        {
            state.Picker = -1;
            title = Plain(state.Menu.Title);
            lines = state.Menu.MenuOptions.Select(option => Plain(option.Text)).ToArray();
            page = state.Page;
            count = state.Menu.MenuOptions.Count;
        }

        int pages = PageCount(count);
        page = Math.Clamp(page, 0, pages - 1);
        if (state.Picker >= 0)
        {
            state.PickerPage = page;
        }
        else
        {
            state.Page = page;
        }

        int start = page * PageSize;
        if (PanoramaHud.AcquireLayout() is { } layout)
        {
            player.PrintToCenterHtml(" ");
            layout.SetHasClassForPlayer(player, "utils-csgo", "csgo-hidden", false);
            string? position = Misc.GetPlayerPosition(player);
            foreach (string name in new[] { "left", "center", "right" })
            {
                layout.SetHasClassForPlayer(player, "utils-csgo", "pos-" + name, name == position);
            }

            layout.SetDialogVariableStringForPlayer(player, "utils-csgo-title", "csgotitle", title);
            for (int i = 0; i < PageSize; i++)
            {
                int index = start + i;
                bool shown = index < count;
                bool disabled = shown && state.Picker < 0 &&
                                index < state.Menu.MenuOptions.Count &&
                                state.Menu.MenuOptions[index].Disabled;
                string rowId = "utils-csgo-row-" + (i + 1);
                layout.SetHasClassForPlayer(player, rowId, "csgo-off", !shown);
                layout.SetHasClassForPlayer(player, rowId, "csgo-disabled", disabled);
                layout.SetDialogVariableStringForPlayer(player, rowId, "csgo" + (i + 1),
                    shown ? $"{i + 1}. {lines[index]}" : "");
            }

            layout.SetHasClassForPlayer(player, "utils-csgo-row-8", "csgo-disabled", !CanGoBack(state));
            layout.SetDialogVariableStringForPlayer(player, "utils-csgo-row-8", "csgo8", "8. " + back);
            layout.SetDialogVariableStringForPlayer(player, "utils-csgo-row-9", "csgo9", "9. " + next);
            layout.SetDialogVariableStringForPlayer(player, "utils-csgo-row-0", "csgo0", "0. " + exit);
            return;
        }

        StringBuilder html = new();
        html.Append("<font color='").Append(TitleColor).Append("' class='fontSize-m'><b>")
            .Append(Esc(title)).Append("</b></font><br>");
        for (int i = 0; i < PageSize; i++)
        {
            int index = start + i;
            if (index >= count)
            {
                break;
            }

            bool disabled = state.Picker < 0 &&
                            index < state.Menu.MenuOptions.Count &&
                            state.Menu.MenuOptions[index].Disabled;
            html.Append("<font color='").Append(disabled ? DisabledColor : ItemColor).Append("'>")
                .Append(i + 1).Append(". ").Append(Esc(lines[index])).Append("</font><br>");
        }

        html.Append("<br>");
        html.Append("<font color='").Append(CanGoBack(state) ? ItemColor : DisabledColor).Append("'>8. ").Append(Esc(back)).Append("</font><br>");
        html.Append("<font color='").Append(ItemColor).Append("'>9. ").Append(Esc(next)).Append("</font><br>");
        html.Append("<font color='").Append(ItemColor).Append("'>0. ").Append(Esc(exit)).Append("</font>");
        player.PrintToCenterHtml(html.ToString());
    }

    private static int ChoiceCount(State state)
    {
        if (state.Picker < 0 || state.Picker >= state.Menu.MenuOptions.Count)
        {
            return 0;
        }

        return MenuOptionKind.TryGetSelect(state.Menu.MenuOptions[state.Picker], out MenuOptionKind.SelectState select)
            ? select.Choices.Length
            : 0;
    }

    private static int PageCount(int count)
    {
        return Math.Max(1, (count + PageSize - 1) / PageSize);
    }

    private static string Plain(string? text)
    {
        return Misc.ColorText(text ?? "", false).Replace(MenuOptionKind.ButtonMark.ToString(), "").Trim();
    }

    private static string Esc(string text)
    {
        return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
    }

    private static void RunCallback(Action action)
    {
        MenuManagerCore? plugin = Control.GetPlugin();
        if (plugin?.Config.IgnoreErrors != true)
        {
            action();
            return;
        }

        try
        {
            action();
        }
        catch (Exception e)
        {
            plugin.Logger.LogInformation(
                "Error was caused in calling plugin: {EMessage}\n=============== STACKTRACE ===============\n{EStackTrace}\n==========================================",
                e.Message, e.StackTrace);
        }
    }

    private sealed class State(MenuInstance menu)
    {
        public MenuInstance Menu { get; } = menu;
        public int Page;
        public int Picker = -1;
        public int PickerPage;
        public float Opened;
        public float LastDraw;
    }
}
