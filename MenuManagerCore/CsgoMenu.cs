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
    // CS:GO menu colours: title and text lines cream, items orange.
    private const string ItemColor = "#f0a63c";
    private const string TitleColor = "#e9e4d0";
    private const string InfoColor = "#e9e4d0";
    private const string KeyOffColor = "#8a8678";

    private static readonly State?[] Slots = new State?[64];

    // True only after the center-HTML fallback drew the menu. PrintToCenterHtml(" ") shows an empty
    // blurred center box in CS2, so the center is cleared only when the fallback really put text there.
    private static readonly bool[] CenterUsed = new bool[64];

    // A line of the panel: Pick = orange (a key does something), otherwise cream text.
    internal readonly record struct Line(string Text, bool Pick);

    private enum RowState
    {
        Pick,
        Info,
        Off,
        Blank
    }

    // css_N and menuselect are registered by NumberKeys, which calls Press while this menu is open.
    internal static void Register(MenuManagerCore plugin)
    {
        plugin.AddTimer(1.0f, Refresh, TimerFlags.REPEAT);
    }

    internal static void Reset()
    {
        Array.Clear(Slots);
        Array.Clear(CenterUsed);
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
        HidePanel(player);
    }

    // Hides the panel whatever it shows (a menu or the number keys check).
    internal static void HidePanel(CCSPlayerController player)
    {
        if (!player.IsValid)
        {
            return;
        }

        ClearCenter(player);
        if (PanoramaHud.ExistingLayout() is { } layout)
        {
            layout.SetHasClassForPlayer(player, "utils-csgo", "csgo-hidden", true);
            layout.SetHasClassForPlayer(player, "utils-csgo", "view-test", false);
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

    internal static void Press(CCSPlayerController player, int key)
    {
        if (!Misc.IsValidPlayer(player) || !IsOpen(player))
        {
            return;
        }

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

    private static bool CanGoNext(State state)
    {
        int count = state.Picker >= 0 ? ChoiceCount(state) : state.Menu.MenuOptions.Count;
        int page = state.Picker >= 0 ? state.PickerPage : state.Page;
        return page < PageCount(count) - 1;
    }

    private static void ShowPanel(CCSCustomHudLayout layout, CCSPlayerController player, bool test)
    {
        layout.SetHasClassForPlayer(player, "utils-csgo", "csgo-hidden", false);
        layout.SetHasClassForPlayer(player, "utils-csgo", "view-test", test);
        string? position = Misc.GetPlayerPosition(player);
        foreach (string name in new[] { "left", "center", "right" })
        {
            layout.SetHasClassForPlayer(player, "utils-csgo", "pos-" + name, name == position);
        }
    }

    private static void SetRow(CCSCustomHudLayout layout, CCSPlayerController player, int key, string text, RowState row)
    {
        string id = "utils-csgo-row-" + key;
        layout.SetHasClassForPlayer(player, id, "csgo-off", row == RowState.Off);
        layout.SetHasClassForPlayer(player, id, "csgo-disabled", row == RowState.Info);
        layout.SetHasClassForPlayer(player, id, "csgo-blank", row == RowState.Blank);
        layout.SetDialogVariableStringForPlayer(player, id, "csgo" + key, text);
    }

    // The number keys check (NumberKeys): ten key boxes, a progress bar and up to seven lines.
    internal static void DrawCheck(CCSPlayerController player, string title, bool[] keys, IReadOnlyList<Line> lines,
        string? back, string exit)
    {
        int slot = player.Slot;
        if (!Misc.IsValidPlayer(player) || slot is < 0 or >= 64)
        {
            return;
        }

        int pressed = keys.Count(k => k);
        if (PanoramaHud.AcquireLayout() is { } layout)
        {
            ClearCenter(player);
            ShowPanel(layout, player, test: true);
            layout.SetDialogVariableStringForPlayer(player, "utils-csgo-title", "csgotitle", title);
            for (int digit = 0; digit <= 9; digit++)
            {
                layout.SetHasClassForPlayer(player, "utils-csgo-k-" + digit, "ck-ok", keys[digit]);
            }

            for (int step = 0; step <= 10; step++)
            {
                layout.SetHasClassForPlayer(player, "utils-csgo-pfill", "p-" + step, step == pressed);
            }

            for (int i = 0; i < PageSize; i++)
            {
                if (i >= lines.Count)
                {
                    SetRow(layout, player, i + 1, "", RowState.Off);
                    continue;
                }

                Line line = lines[i];
                SetRow(layout, player, i + 1, line.Text,
                    line.Text.Length == 0 ? RowState.Blank : line.Pick ? RowState.Pick : RowState.Info);
            }

            SetRow(layout, player, 8, back ?? "", back == null ? RowState.Blank : RowState.Pick);
            SetRow(layout, player, 9, "", RowState.Blank);
            SetRow(layout, player, 0, exit, RowState.Pick);
            return;
        }

        StringBuilder html = new();
        html.Append("<font color='").Append(TitleColor).Append("' class='fontSize-m'><b>")
            .Append(Esc(title)).Append("</b></font><br>");
        foreach (int digit in new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 })
        {
            html.Append("<font color='").Append(keys[digit] ? ItemColor : KeyOffColor).Append("'>[")
                .Append(digit).Append("]</font> ");
        }

        html.Append("<br>");
        foreach (Line line in lines)
        {
            html.Append("<font color='").Append(line.Pick ? ItemColor : InfoColor).Append("'>")
                .Append(Esc(line.Text)).Append("</font><br>");
        }

        if (back != null)
        {
            html.Append("<font color='").Append(ItemColor).Append("'>").Append(Esc(back)).Append("</font><br>");
        }

        html.Append("<font color='").Append(ItemColor).Append("'>").Append(Esc(exit)).Append("</font>");
        player.PrintToCenterHtml(html.ToString());
        CenterUsed[slot] = true;
    }

    private static void Next(CCSPlayerController player, State state)
    {
        int count = state.Picker >= 0 ? ChoiceCount(state) : state.Menu.MenuOptions.Count;
        int page = state.Picker >= 0 ? state.PickerPage : state.Page;
        if (page >= PageCount(count) - 1)
        {
            return;
        }

        Control.PlaySound(player, Control.NextPageSound());
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
            ClearCenter(player);
            ShowPanel(layout, player, test: false);
            layout.SetDialogVariableStringForPlayer(player, "utils-csgo-title", "csgotitle", title);
            for (int i = 0; i < PageSize; i++)
            {
                int index = start + i;
                bool shown = index < count;
                bool disabled = shown && state.Picker < 0 &&
                                index < state.Menu.MenuOptions.Count &&
                                state.Menu.MenuOptions[index].Disabled;
                SetRow(layout, player, i + 1, shown ? $"{i + 1}. {lines[index]}" : "",
                    !shown ? RowState.Off : disabled ? RowState.Info : RowState.Pick);
            }

            // As in CS:GO: a key that does nothing leaves an empty line.
            SetRow(layout, player, 8, "8. " + back, CanGoBack(state) ? RowState.Pick : RowState.Blank);
            SetRow(layout, player, 9, "9. " + next, CanGoNext(state) ? RowState.Pick : RowState.Blank);
            SetRow(layout, player, 0, "0. " + exit, RowState.Pick);
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
            html.Append("<font color='").Append(disabled ? InfoColor : ItemColor).Append("'>")
                .Append(i + 1).Append(". ").Append(Esc(lines[index])).Append("</font><br>");
        }

        html.Append("<br>");
        html.Append(CanGoBack(state) ? $"<font color='{ItemColor}'>8. {Esc(back)}</font>" : "").Append("<br>");
        html.Append(CanGoNext(state) ? $"<font color='{ItemColor}'>9. {Esc(next)}</font>" : "").Append("<br>");
        html.Append("<font color='").Append(ItemColor).Append("'>0. ").Append(Esc(exit)).Append("</font>");
        player.PrintToCenterHtml(html.ToString());
        CenterUsed[player.Slot] = true;
    }

    private static void ClearCenter(CCSPlayerController player)
    {
        int slot = player.Slot;
        if (slot is < 0 or >= 64 || !CenterUsed[slot])
        {
            return;
        }

        CenterUsed[slot] = false;
        player.PrintToCenterHtml(" ");
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
