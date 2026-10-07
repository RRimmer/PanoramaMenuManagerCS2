using System.Diagnostics.CodeAnalysis;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Extensions;
using CounterStrikeSharp.API.Modules.Menu;
using MenuManager;
using Microsoft.Extensions.Logging;

namespace MenuManagerCore;

internal static class PanoramaHud
{
    internal const string LayoutPath = "panorama/layout/custom_game/menu_ui.xml";
    private const string DialogId = "utils-dialog";
    private const int ItemsPerPage = 6;
    private const int MaxSelectOptions = 12;
    private static readonly string[] ItemKinds = ["k-empty", "k-value", "k-submenu", "k-toggle", "k-select", "k-choice"];
    private static readonly string[] Positions = ["left", "center", "right"];
    private static CCSCustomHudLayout? _layout;
    private static readonly SlotMenu?[] Slots = new SlotMenu?[64];
    private static readonly Dictionary<string, int>?[] PageMemory = new Dictionary<string, int>?[64];
    private static readonly Dictionary<string, int>?[] CursorMemory = new Dictionary<string, int>?[64];
    // Kept outside SlotMenu: a WASD menu that reopens itself replaces its state,
    // and the player has to stay frozen across that.
    private static readonly bool[] Frozen = new bool[64];
    private static readonly int[] NoticeToken = new int[64];
    private static readonly string[] NoticeClasses = ["ntf-success", "ntf-warning", "ntf-error"];

    internal static void Reset()
    {
        _layout = null;
        Array.Clear(Slots);
        Array.Clear(PageMemory);
        Array.Clear(CursorMemory);
        Array.Clear(Frozen);
    }

    internal static void Warmup()
    {
        Server.NextFrame(() => { Server.NextFrame(static () => EnsureLayout()); });
    }

    internal static void Close(CCSPlayerController? eventPlayer)
    {
        if (eventPlayer == null || !eventPlayer.IsValid)
        {
            return;
        }

        Hide(eventPlayer, forgetPages: true);
        HideNotice(eventPlayer);
        if (eventPlayer.Slot is >= 0 and < 64)
        {
            Frozen[eventPlayer.Slot] = false;
        }
    }

    internal static void CloseMenu(CCSPlayerController player)
    {
        if (IsOpen(player))
        {
            Hide(player, forgetPages: true);
        }
    }

    internal static bool IsOpen(CCSPlayerController player)
    {
        return player is { IsValid: true, Slot: >= 0 and < 64 } && Slots[player.Slot] != null;
    }

    internal static bool IsOwnLayout(CCSCustomHudLayout layout)
    {
        return _layout != null && layout.Handle == _layout.Handle;
    }

    internal static void ReleaseAll()
    {
        for (int slot = 0; slot < Frozen.Length; slot++)
        {
            if (!Frozen[slot])
            {
                continue;
            }

            Frozen[slot] = false;
            CCSPlayerController? player = Utilities.GetPlayerFromSlot(slot);
            if (player != null && player.IsValid)
            {
                Control.Unfreeze(player);
            }
        }
    }

    internal static void Notify(CCSPlayerController player, string title, string text, MenuNotice notice)
    {
        if (!Misc.IsValidPlayer(player) || !Misc.GetPlayerNotifications(player) || !EnsureLayout() || _layout == null)
        {
            return;
        }

        int slot = player.Slot;
        if (slot is < 0 or >= 64)
        {
            return;
        }

        int token = ++NoticeToken[slot];
        string kind = notice switch
        {
            MenuNotice.Warning => "ntf-warning",
            MenuNotice.Error => "ntf-error",
            _ => "ntf-success"
        };
        string icon = notice switch
        {
            MenuNotice.Warning => "!",
            MenuNotice.Error => "x",
            _ => "+"
        };

        ApplyLook(_layout, player);
        foreach (string name in NoticeClasses)
        {
            _layout.SetHasClassForPlayer(player, "utils-notify", name, name == kind);
        }

        _layout.SetHasClassForPlayer(player, "utils-notify", "ntf-hidden", false);
        _layout.SetDialogVariableStringForPlayer(player, "utils-notify-icon", "ntficon", icon);
        _layout.SetDialogVariableStringForPlayer(player, "utils-notify-title", "ntftitle", title);
        _layout.SetDialogVariableStringForPlayer(player, "utils-notify-text", "ntftext", text);

        Control.GetPlugin()?.AddTimer(3.2f, () =>
        {
            if (NoticeToken[slot] != token || _layout is not { IsValid: true } || !player.IsValid)
            {
                return;
            }

            _layout.SetHasClassForPlayer(player, "utils-notify", "ntf-hidden", true);
        });
    }

    internal static bool Show(CCSPlayerController player, MenuInstance menu, bool wasd)
    {
        if (!Misc.IsValidPlayer(player))
        {
            return false;
        }

        CsgoMenu.Close(player);
        Control.CloseMenu(player);
        int slot = player.Slot;
        if (Slots[slot] is { } open)
        {
            Remember(slot, open.Menu.Title, open.Page);
            if (open.Wasd)
            {
                RememberCursor(slot, open.Menu.Title, open.Cursor);
            }
        }

        SlotMenu state = new(menu) { Wasd = wasd };
        if (wasd)
        {
            state.Cursor = Math.Clamp(RecallCursor(slot, menu.Title), 0, Math.Max(0, menu.MenuOptions.Count - 1));
            state.Page = state.Cursor / ItemsPerPage;
            state.LastButtons = ReadButtons(player);
            state.LastInput = Server.CurrentTime;
        }
        else
        {
            state.Page = Recall(slot, menu.Title);
        }

        Slots[slot] = state;
        // The say command and the HUD update must not share a frame. CS2 then stalls
        // CCLCMsg_Move and logs it as NETWORK_DISCONNECT_KICKED.
        Control.GetPlugin()?.AddTimer(Math.Max(0.01f, Control.GetPlugin()?.Config.HudOpenDelay ?? 0.2f), () =>
        {
            if (!Misc.IsValidPlayer(player) || !ReferenceEquals(Slots[player.Slot], state))
            {
                return;
            }

            if (!EnsureLayout())
            {
                Slots[player.Slot] = null;
                return;
            }

            Draw(player, captureInput: false);
            if (state.Wasd)
            {
                return;
            }

            Control.GetPlugin()?.AddTimer(Math.Max(0.01f, Control.GetPlugin()?.Config.HudOpenDelay ?? 0.2f), () =>
            {
                if (_layout is not { IsValid: true } || !Misc.IsValidPlayer(player) ||
                    !ReferenceEquals(Slots[player.Slot], state) || state.Wasd)
                {
                    return;
                }

                _layout.SetInputCaptureEnabled(player, true);
            });
        });
        return true;
    }

    internal static void OnClick(CCSPlayerController player, CCSCustomHudLayout customLayout, string buttonId)
    {
        if (!customLayout.IsValid)
        {
            return;
        }

        if (_layout == null || customLayout.Handle != _layout.Handle)
        {
            PisexLayout.OnClick(player, customLayout);
            return;
        }

        if (!Misc.IsValidPlayer(player))
        {
            return;
        }

        int slot = player.Slot;
        SlotMenu? state = Slots[slot];
        if (state == null)
        {
            return;
        }

        if (state.Picker >= 0)
        {
            OnPickerClick(player, state, buttonId);
            return;
        }

        MenuInstance menu = state.Menu;
        switch (buttonId)
        {
            case "utils-closeBtn":
                Hide(player, forgetPages: true);
                return;
            case "utils-prev":
                if (state.Page > 0)
                {
                    state.Page--;
                    state.OpenSelect = -1;
                    Draw(player, captureInput: true);
                }

                return;
            case "utils-next":
                if (state.Page < PageCount(menu.MenuOptions.Count) - 1)
                {
                    state.Page++;
                    state.OpenSelect = -1;
                    Draw(player, captureInput: true);
                }

                return;
            case "utils-back":
                if (state.Page > 0)
                {
                    state.Page--;
                    Draw(player, captureInput: true);
                    return;
                }

                Hide(player, forgetPages: false);
                if (menu.BackAction != null)
                {
                    RunCallback(() => menu.BackAction(player));
                }

                return;
        }

        if (buttonId.StartsWith("utils-opt-", StringComparison.Ordinal))
        {
            PickSelect(player, state, buttonId);
            return;
        }

        if (!buttonId.StartsWith("utils-item-", StringComparison.Ordinal))
        {
            return;
        }

        if (!int.TryParse(buttonId["utils-item-".Length..], out int visible))
        {
            return;
        }

        int index = state.Page * ItemsPerPage + visible;
        if (index < 0 || index >= menu.MenuOptions.Count || menu.MenuOptions[index].Disabled)
        {
            return;
        }

        state.Cursor = index;
        Activate(player, state, index);
    }

    internal static void OnTick()
    {
        MenuManagerCore? plugin = Control.GetPlugin();
        if (plugin == null)
        {
            return;
        }

        for (int slot = 0; slot < Slots.Length; slot++)
        {
            SlotMenu? state = Slots[slot];
            bool wasd = state is { Wasd: true };
            if (!wasd && !Frozen[slot])
            {
                continue;
            }

            CCSPlayerController? player = Utilities.GetPlayerFromSlot(slot);
            if (player == null || !Misc.IsValidPlayer(player))
            {
                Frozen[slot] = false;
                if (wasd)
                {
                    Slots[slot] = null;
                }

                continue;
            }

            if (state is not { Wasd: true })
            {
                Frozen[slot] = false;
                Control.Unfreeze(player);
                continue;
            }

            if (plugin.Config.StopingUser && Control.Freeze(player))
            {
                Frozen[slot] = true;
            }

            if (Server.CurrentTime - state.LastInput > plugin.Config.MenuTime)
            {
                Control.PlaySound(player, plugin.Config.SoundExit);
                Hide(player, forgetPages: true);
                continue;
            }

            PlayerButtons buttons = ReadButtons(player);
            PlayerButtons pressed = buttons & ~state.LastButtons;
            state.LastButtons = buttons;
            if (pressed != 0)
            {
                OnKeys(player, state, pressed, plugin);
            }
        }
    }

    internal static void ApplyLook(CCSCustomHudLayout layout, CCSPlayerController player)
    {
        string? position = Misc.GetPlayerPosition(player);
        foreach (string name in Positions)
        {
            layout.SetHasClassForPlayer(player, DialogId, "pos-" + name, name == position);
        }

    }

    private static void OnKeys(CCSPlayerController player, SlotMenu state, PlayerButtons pressed, MenuManagerCore plugin)
    {
        ButtonsConfig keys = plugin.Config.ButtonsConfig;
        if (Has(pressed, keys.UpButton))
        {
            Move(player, state, -1);
        }
        else if (Has(pressed, keys.DownButton))
        {
            Move(player, state, 1);
        }
        else if (Has(pressed, keys.LeftButton) || Has(pressed, keys.BackButton))
        {
            Back(player, state);
        }
        else if (Has(pressed, keys.RightButton) || Has(pressed, keys.SelectButton))
        {
            Select(player, state);
        }
        else if (Has(pressed, keys.ExitButton))
        {
            Control.PlaySound(player, plugin.Config.SoundExit);
            Hide(player, forgetPages: true);
        }
        else
        {
            return;
        }

        state.LastInput = Server.CurrentTime;
    }

    private static bool Has(PlayerButtons pressed, PlayerButtons key)
    {
        return key != 0 && (pressed & key) != 0;
    }

    private static void Move(CCSPlayerController player, SlotMenu state, int delta)
    {
        bool picker = TryGetPicker(state, out _, out MenuOptionKind.SelectState? select);
        int count = picker ? select!.Choices.Length : state.Menu.MenuOptions.Count;
        int cursor = picker ? state.PickerCursor : state.Cursor;
        int next = cursor + delta;
        if (next < 0 || next >= count)
        {
            return;
        }

        Control.PlaySound(player, Control.GetPlugin()?.Config.SoundScroll ?? "");
        int page = picker ? state.PickerPage : state.Page;
        if (picker)
        {
            state.PickerCursor = next;
        }
        else
        {
            state.Cursor = next;
        }

        if (next / ItemsPerPage != page || _layout is not { IsValid: true })
        {
            Draw(player, captureInput: false);
            return;
        }

        _layout.SetHasClassForPlayer(player, $"utils-item-{cursor - page * ItemsPerPage}", "is-focus", false);
        _layout.SetHasClassForPlayer(player, $"utils-item-{next - page * ItemsPerPage}", "is-focus", true);
        if (!picker)
        {
            RememberCursor(player.Slot, state.Menu.Title, next);
        }
    }

    private static void Select(CCSPlayerController player, SlotMenu state)
    {
        MenuManagerCore? plugin = Control.GetPlugin();
        if (TryGetPicker(state, out _, out _))
        {
            Control.PlaySound(player, plugin?.Config.SoundClick ?? "");
            Pick(player, state, state.PickerCursor);
            return;
        }

        int index = state.Cursor;
        if (index < 0 || index >= state.Menu.MenuOptions.Count)
        {
            return;
        }

        if (state.Menu.MenuOptions[index].Disabled)
        {
            Control.PlaySound(player, plugin?.Config.SoundDisabled ?? "");
            return;
        }

        Control.PlaySound(player, plugin?.Config.SoundClick ?? "");
        Activate(player, state, index);
    }

    private static void Back(CCSPlayerController player, SlotMenu state)
    {
        if (state.Picker >= 0)
        {
            Control.PlaySound(player, Control.GetPlugin()?.Config.SoundBack ?? "");
            state.Picker = -1;
            Draw(player, captureInput: false);
            return;
        }

        MenuInstance menu = state.Menu;
        int backOption = FindBackOption(menu);
        if (backOption >= 0)
        {
            Activate(player, state, backOption);
            return;
        }

        Control.PlaySound(player, Control.GetPlugin()?.Config.SoundBack ?? "");
        if (menu.BackAction == null)
        {
            Hide(player, forgetPages: true);
            return;
        }

        Hide(player, forgetPages: false);
        RunCallback(() => menu.BackAction(player));
    }

    private static int FindBackOption(MenuInstance menu)
    {
        for (int i = 0; i < menu.MenuOptions.Count; i++)
        {
            ChatMenuOption option = menu.MenuOptions[i];
            if (!option.Disabled && IsBackLabel(option.Text))
            {
                return i;
            }
        }

        return -1;
    }

    private static bool IsBackLabel(string? text)
    {
        string visible = VisibleText(text ?? "").Trim();
        int start = 0;
        while (start < visible.Length && !char.IsLetter(visible[start]))
        {
            start++;
        }

        int end = visible.Length;
        while (end > start && !char.IsLetter(visible[end - 1]))
        {
            end--;
        }

        if (end <= start)
        {
            return false;
        }

        string word = visible[start..end];
        return word.Equals("назад", StringComparison.OrdinalIgnoreCase)
               || word.Equals("back", StringComparison.OrdinalIgnoreCase)
               || word.Equals("geri", StringComparison.OrdinalIgnoreCase)
               || word.Equals("返回", StringComparison.Ordinal);
    }

    private static void Activate(CCSPlayerController player, SlotMenu state, int index)
    {
        MenuInstance menu = state.Menu;
        ChatMenuOption option = menu.MenuOptions[index];
        if (MenuOptionKind.TryGetSelect(option, out MenuOptionKind.SelectState select) && select.Choices.Length > 0)
        {
            if (!state.Wasd && select.Choices.Length <= MaxSelectOptions)
            {
                int visible = index - state.Page * ItemsPerPage;
                state.OpenSelect = state.OpenSelect == visible ? -1 : visible;
                Draw(player, captureInput: true);
                return;
            }

            OpenPicker(player, state, index, select);
            return;
        }

        int slot = player.Slot;
        state.OpenSelect = -1;
        Remember(slot, menu.Title, state.Page);
        if (state.Wasd)
        {
            RememberCursor(slot, menu.Title, state.Cursor);
        }

        if (menu.PostSelectAction != PostSelectAction.Nothing)
        {
            Hide(player, forgetPages: false);
        }

        bool isToggle = MenuOptionKind.TryGetToggle(option, out bool wasOn);
        RunCallback(() => option.OnSelect(player, option));
        if (menu.PostSelectAction == PostSelectAction.Reset && menu.ResetAction != null)
        {
            Server.NextFrame(() => menu.ResetAction(player));
        }

        // Keep the same menu open: flip toggle visuals without a full Close/Open.
        if (isToggle &&
            menu.PostSelectAction == PostSelectAction.Nothing &&
            Slots[player.Slot] is { } after &&
            ReferenceEquals(after.Menu, menu))
        {
            bool next = !wasOn;
            MenuOptionKind.SetToggle(option, next);
            string label = MenuOptionKind.WithoutState(VisibleText(option.Text ?? ""));
            option.Text = MenuOptionKind.WithState(label, next);
            Draw(player, captureInput: true);
        }
    }

    private static void OpenPicker(CCSPlayerController player, SlotMenu state, int index, MenuOptionKind.SelectState select)
    {
        state.OpenSelect = -1;
        state.Picker = index;
        int current = Array.FindIndex(select.Choices,
            choice => choice.Equals(select.Value, StringComparison.OrdinalIgnoreCase));
        state.PickerCursor = Math.Max(0, current);
        state.PickerPage = state.PickerCursor / ItemsPerPage;
        Draw(player, captureInput: true);
    }

    private static void OnPickerClick(CCSPlayerController player, SlotMenu state, string buttonId)
    {
        if (!TryGetPicker(state, out _, out MenuOptionKind.SelectState? select))
        {
            state.Picker = -1;
            Draw(player, captureInput: true);
            return;
        }

        switch (buttonId)
        {
            case "utils-closeBtn":
                Hide(player, forgetPages: true);
                return;
            case "utils-back":
                state.Picker = -1;
                Draw(player, captureInput: true);
                return;
            case "utils-prev":
                if (state.PickerPage > 0)
                {
                    state.PickerPage--;
                    Draw(player, captureInput: true);
                }

                return;
            case "utils-next":
                if (state.PickerPage < PageCount(select.Choices.Length) - 1)
                {
                    state.PickerPage++;
                    Draw(player, captureInput: true);
                }

                return;
        }

        if (!buttonId.StartsWith("utils-item-", StringComparison.Ordinal) ||
            !int.TryParse(buttonId["utils-item-".Length..], out int visible) ||
            visible is < 0 or >= ItemsPerPage)
        {
            return;
        }

        int choice = state.PickerPage * ItemsPerPage + visible;
        if (choice < select.Choices.Length)
        {
            Pick(player, state, choice);
        }
    }

    private static void Pick(CCSPlayerController player, SlotMenu state, int choice)
    {
        if (!TryGetPicker(state, out ChatMenuOption? option, out MenuOptionKind.SelectState? select))
        {
            state.Picker = -1;
            Draw(player, captureInput: true);
            return;
        }

        state.Picker = -1;
        if (choice < 0 || choice >= select.Choices.Length)
        {
            Draw(player, captureInput: true);
            return;
        }

        int slot = player.Slot;
        MenuInstance menu = state.Menu;
        Remember(slot, menu.Title, state.Page);
        if (state.Wasd)
        {
            RememberCursor(slot, menu.Title, state.Cursor);
        }

        select.Value = select.Choices[choice];
        RunCallback(() => select.OnSelect?.Invoke(player, option, choice));
        if (Slots[slot] == state)
        {
            Draw(player, captureInput: true);
        }
    }

    private static bool TryGetPicker(SlotMenu state, [NotNullWhen(true)] out ChatMenuOption? option,
        [NotNullWhen(true)] out MenuOptionKind.SelectState? select)
    {
        option = null;
        select = null;
        if (state.Picker < 0 || state.Picker >= state.Menu.MenuOptions.Count)
        {
            return false;
        }

        option = state.Menu.MenuOptions[state.Picker];
        if (MenuOptionKind.TryGetSelect(option, out MenuOptionKind.SelectState found) && found.Choices.Length > 0)
        {
            select = found;
            return true;
        }

        option = null;
        return false;
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

    private static PlayerButtons ReadButtons(CCSPlayerController player)
    {
        return player.Pawn.Value?.MovementServices == null ? 0 : player.Buttons;
    }

    private static string HintText()
    {
        MenuManagerCore? plugin = Control.GetPlugin();
        if (plugin == null)
        {
            return "";
        }

        ButtonsConfig keys = plugin.Config.ButtonsConfig;
        return plugin.Localizer["menumanager.wasd_hint", KeyName(keys.UpButton), KeyName(keys.DownButton),
            $"{KeyName(keys.RightButton)}/{KeyName(keys.SelectButton)}", KeyName(keys.LeftButton),
            KeyName(keys.ExitButton)];
    }

    private static string KeyName(PlayerButtons button)
    {
        return button switch
        {
            PlayerButtons.Forward => "W",
            PlayerButtons.Back => "S",
            PlayerButtons.Moveleft => "A",
            PlayerButtons.Moveright => "D",
            PlayerButtons.Use => "E",
            PlayerButtons.Reload => "R",
            PlayerButtons.Duck => "Ctrl",
            PlayerButtons.Jump => "Space",
            PlayerButtons.Speed => "Shift",
            PlayerButtons.Attack => "Mouse1",
            PlayerButtons.Attack2 => "Mouse2",
            PlayerButtons.Scoreboard => "Tab",
            PlayerButtons.Inspect => "F",
            _ => button.ToString()
        };
    }

    internal static CCSCustomHudLayout? AcquireLayout()
    {
        return EnsureLayout() ? _layout : null;
    }

    internal static CCSCustomHudLayout? ExistingLayout()
    {
        return _layout is { IsValid: true } ? _layout : null;
    }

    private static bool EnsureLayout()
    {
        if (_layout is { IsValid: true })
        {
            return true;
        }

        CCSCustomHudLayout? layout = Utilities.CreateEntityByName<CCSCustomHudLayout>("custom_hud_layout");
        if (layout == null || layout.Handle == IntPtr.Zero)
        {
            Control.GetPlugin()?.Logger.LogError("Panorama menu: custom_hud_layout was not created");
            return false;
        }

        layout.StrLayout = LayoutPath;
        layout.DispatchSpawn();
        if (!layout.IsValid)
        {
            Control.GetPlugin()?.Logger.LogError("Panorama menu: custom_hud_layout did not spawn");
            return false;
        }

        _layout = layout;
        return true;
    }

    private static void Draw(CCSPlayerController player, bool captureInput)
    {
        if (_layout == null || !_layout.IsValid)
        {
            return;
        }

        SlotMenu? state = player.Slot is >= 0 and < 64 ? Slots[player.Slot] : null;
        if (state == null)
        {
            Hide(player, forgetPages: true);
            return;
        }

        ApplyLook(_layout, player);
        _layout.SetHasClassForPlayer(player, DialogId, "mode-wasd", state.Wasd);
        _layout.SetHasClassForPlayer(player, DialogId, "utils-dismissed", false);
        if (state.Wasd)
        {
            _layout.SetInputCaptureEnabled(player, false);
        }
        else if (captureInput)
        {
            _layout.SetInputCaptureEnabled(player, true);
        }

        _layout.SetDialogVariableStringForPlayer(player, "utils-desc", "desc", "");
        _layout.SetDialogVariableStringForPlayer(player, "utils-hint", "hint", state.Wasd ? HintText() : "");

        if (TryGetPicker(state, out ChatMenuOption? option, out MenuOptionKind.SelectState? select))
        {
            DrawPicker(player, state, option, select);
            return;
        }

        state.Picker = -1;
        DrawRows(player, state);
    }

    private static void DrawRows(CCSPlayerController player, SlotMenu state)
    {
        MenuInstance menu = state.Menu;
        int count = menu.MenuOptions.Count;
        int pages = PageCount(count);
        if (state.Wasd)
        {
            state.Cursor = Math.Clamp(state.Cursor, 0, Math.Max(0, count - 1));
            state.Page = state.Cursor / ItemsPerPage;
        }

        state.Page = Math.Clamp(state.Page, 0, pages - 1);
        int slot = player.Slot;
        Remember(slot, menu.Title, state.Page);
        if (state.Wasd)
        {
            RememberCursor(slot, menu.Title, state.Cursor);
        }

        DrawHeader(player, Misc.ColorText(menu.Title, false), state.Page, pages,
            menu.BackAction == null && state.Page == 0);

        for (int i = 0; i < ItemsPerPage; i++)
        {
            int index = state.Page * ItemsPerPage + i;
            if (index >= count)
            {
                SetRow(player, i, "k-empty", "", "");
                FillSelect(player, i, null, "");
                continue;
            }

            ChatMenuOption option = menu.MenuOptions[index];
            string raw = option.Text ?? "";
            bool linked = raw.Contains(MenuOptionKind.ButtonMark);
            string visible = MenuOptionKind.WithoutState(VisibleText(raw));
            string kind = "k-submenu";
            bool on = false;
            bool dropdown = false;
            string label = visible;
            string value = "";
            MenuOptionKind.SelectState? select = null;
            if (MenuOptionKind.TryGetToggle(option, out on))
            {
                kind = "k-toggle";
            }
            else if (MenuOptionKind.TryGetSelect(option, out MenuOptionKind.SelectState found) && found.Choices.Length > 0)
            {
                kind = "k-select";
                dropdown = !state.Wasd && found.Choices.Length <= MaxSelectOptions;
                select = dropdown ? found : null;
                value = found.Value;
            }
            else if (!linked && MenuOptionKind.TryParseToggleLabel(visible, out string parsed, out on))
            {
                kind = "k-toggle";
                label = parsed;
            }

            bool rowOpen = dropdown && state.OpenSelect == i;
            SetRow(player, i, kind, label, value,
                on: kind == "k-toggle" && on,
                open: rowOpen,
                disabled: option.Disabled,
                sub: kind == "k-select" && !dropdown,
                focus: state.Wasd && index == state.Cursor);
            if (rowOpen)
            {
                FillSelect(player, i, select, value);
            }
        }

        if (state.FilledSelect != state.OpenSelect)
        {
            if (state.FilledSelect >= 0)
            {
                FillSelect(player, state.FilledSelect, null, "");
            }

            state.FilledSelect = state.OpenSelect;
        }
    }

    private static void DrawPicker(CCSPlayerController player, SlotMenu state, ChatMenuOption option,
        MenuOptionKind.SelectState select)
    {
        int count = select.Choices.Length;
        int pages = PageCount(count);
        if (state.Wasd)
        {
            state.PickerCursor = Math.Clamp(state.PickerCursor, 0, count - 1);
            state.PickerPage = state.PickerCursor / ItemsPerPage;
        }

        state.PickerPage = Math.Clamp(state.PickerPage, 0, pages - 1);
        DrawHeader(player, MenuOptionKind.WithoutState(VisibleText(option.Text ?? "")), state.PickerPage, pages, hideBack: false);

        for (int i = 0; i < ItemsPerPage; i++)
        {
            int choice = state.PickerPage * ItemsPerPage + i;
            if (choice >= count)
            {
                SetRow(player, i, "k-empty", "", "");
                FillSelect(player, i, null, "");
                continue;
            }

            string choiceText = select.Choices[choice];
            bool picked = choiceText.Equals(select.Value, StringComparison.OrdinalIgnoreCase);
            string label = MenuOptionKind.WithoutState(choiceText);
            SetRow(player, i, "k-choice", label, "",
                picked: picked,
                focus: state.Wasd && choice == state.PickerCursor);
            FillSelect(player, i, null, "");
        }
    }

    private static void DrawHeader(CCSPlayerController player, string title, int page, int pages, bool hideBack)
    {
        if (_layout == null)
        {
            return;
        }

        _layout.SetDialogVariableStringForPlayer(player, "utils-title", "title", title);
        _layout.SetDialogVariableStringForPlayer(player, "utils-page", "page", $"{page + 1} / {pages}");
        _layout.SetHasClassForPlayer(player, "utils-back-anchor", "utils-hide", hideBack);
        _layout.SetHasClassForPlayer(player, "utils-prev", "utils-disabled", page <= 0);
        _layout.SetHasClassForPlayer(player, "utils-next", "utils-disabled", page >= pages - 1);
    }

    private static void SetRow(CCSPlayerController player, int visible, string kind, string label, string value,
        bool on = false, bool open = false, bool disabled = false, bool sub = false, bool focus = false,
        bool picked = false)
    {
        if (_layout == null)
        {
            return;
        }

        string itemId = $"utils-item-{visible}";
        SetKind(player, itemId, kind);
        _layout.SetHasClassForPlayer(player, itemId, "is-on", on);
        _layout.SetHasClassForPlayer(player, itemId, "is-open", open);
        _layout.SetHasClassForPlayer(player, itemId, "is-up", open && visible >= 3);
        SetSlotOpen(player, visible, open);
        _layout.SetHasClassForPlayer(player, itemId, "is-disabled", disabled);
        _layout.SetHasClassForPlayer(player, itemId, "is-sub", sub);
        _layout.SetHasClassForPlayer(player, itemId, "is-focus", focus);
        _layout.SetHasClassForPlayer(player, itemId, "is-picked", picked);
        _layout.SetDialogVariableStringForPlayer(player, itemId, "label", label);
        _layout.SetDialogVariableStringForPlayer(player, itemId, "value", value);
    }

    private static string VisibleText(string raw)
    {
        return Misc.ColorText(raw, false).Replace(MenuOptionKind.ButtonMark.ToString(), "");
    }

    private static void PickSelect(CCSPlayerController player, SlotMenu state, string buttonId)
    {
        string[] parts = buttonId.Split('-');
        if (parts.Length != 4 ||
            !int.TryParse(parts[2], out int visible) ||
            !int.TryParse(parts[3], out int choice))
        {
            return;
        }

        MenuInstance menu = state.Menu;
        int index = state.Page * ItemsPerPage + visible;
        if (index < 0 || index >= menu.MenuOptions.Count)
        {
            return;
        }

        ChatMenuOption option = menu.MenuOptions[index];
        if (!MenuOptionKind.TryGetSelect(option, out MenuOptionKind.SelectState select) ||
            choice < 0 || choice >= select.Choices.Length)
        {
            return;
        }

        int slot = player.Slot;
        state.OpenSelect = -1;
        Remember(slot, menu.Title, state.Page);
        select.Value = select.Choices[choice];
        RunCallback(() => select.OnSelect?.Invoke(player, option, choice));
        if (Slots[slot]?.Menu == menu)
        {
            Draw(player, captureInput: true);
        }
    }

    private static void FillSelect(CCSPlayerController player, int visible, MenuOptionKind.SelectState? select, string current)
    {
        if (_layout == null)
        {
            return;
        }

        for (int choice = 0; choice < MaxSelectOptions; choice++)
        {
            string optionId = $"utils-opt-{visible}-{choice}";
            bool shown = select != null && choice < select.Choices.Length;
            _layout.SetHasClassForPlayer(player, optionId, "o-show", shown);
            string raw = shown ? select!.Choices[choice] : "";
            bool picked = shown && raw.Equals(current, StringComparison.OrdinalIgnoreCase);
            string label = shown ? MenuOptionKind.WithoutState(raw) : "";
            _layout.SetHasClassForPlayer(player, optionId, "utils-sel", picked);
            _layout.SetDialogVariableStringForPlayer(player, $"utils-opt-label-{visible}-{choice}", $"o{visible}_{choice}", label);
        }
    }

    private static void Remember(int slot, string title, int page)
    {
        Store(PageMemory, slot, title, page);
    }

    private static int Recall(int slot, string title)
    {
        return Load(PageMemory, slot, title);
    }

    private static void RememberCursor(int slot, string title, int cursor)
    {
        Store(CursorMemory, slot, title, cursor);
    }

    private static int RecallCursor(int slot, string title)
    {
        return Load(CursorMemory, slot, title);
    }

    private static void Store(Dictionary<string, int>?[] memory, int slot, string title, int value)
    {
        if (slot is < 0 or >= 64 || string.IsNullOrEmpty(title))
        {
            return;
        }

        Dictionary<string, int> values = memory[slot] ??= new Dictionary<string, int>(StringComparer.Ordinal);
        values[title] = value;
    }

    private static int Load(Dictionary<string, int>?[] memory, int slot, string title)
    {
        if (slot is < 0 or >= 64 || string.IsNullOrEmpty(title))
        {
            return 0;
        }

        Dictionary<string, int>? values = memory[slot];
        return values != null && values.TryGetValue(title, out int value) ? value : 0;
    }

    private static void Forget(int slot)
    {
        if (slot is >= 0 and < 64)
        {
            PageMemory[slot] = null;
            CursorMemory[slot] = null;
        }
    }

    private static void HideNotice(CCSPlayerController player)
    {
        if (player.Slot is >= 0 and < 64)
        {
            NoticeToken[player.Slot]++;
        }

        if (_layout is not { IsValid: true } || !player.IsValid)
        {
            return;
        }

        _layout.SetHasClassForPlayer(player, "utils-notify", "ntf-hidden", true);
    }

    private static void Hide(CCSPlayerController player, bool forgetPages)
    {
        if (player.Slot is >= 0 and < 64)
        {
            if (forgetPages)
            {
                Forget(player.Slot);
            }

            Slots[player.Slot] = null;
        }

        if (_layout is not { IsValid: true })
        {
            return;
        }

        _layout.SetHasClassForPlayer(player, DialogId, "utils-dismissed", true);
        _layout.SetInputCaptureEnabled(player, false);
        if (player.Slot is >= 0 and < 64 && Frozen[player.Slot])
        {
            Frozen[player.Slot] = false;
            Control.Unfreeze(player);
        }
    }

    private static void SetSlotOpen(CCSPlayerController player, int visible, bool open)
    {
        if (_layout == null)
        {
            return;
        }

        string slotId = $"utils-slot-{visible}";
        _layout.SetHasClassForPlayer(player, slotId, "is-open", open);
        _layout.SetHasClassForPlayer(player, slotId, "is-up", open && visible >= 3);
    }

    private static void SetKind(CCSPlayerController player, string itemId, string kind)
    {
        if (_layout == null)
        {
            return;
        }

        foreach (string name in ItemKinds)
        {
            _layout.SetHasClassForPlayer(player, itemId, name, name == kind);
        }
    }

    private static int PageCount(int count)
    {
        return (Math.Max(1, count) + ItemsPerPage - 1) / ItemsPerPage;
    }

    private sealed class SlotMenu(MenuInstance menu)
    {
        public readonly MenuInstance Menu = menu;
        public bool Wasd;
        public int Page;
        public int OpenSelect = -1;
        public int FilledSelect = -1;
        public int Cursor;
        public int Picker = -1;
        public int PickerPage;
        public int PickerCursor;
        public PlayerButtons LastButtons;
        public float LastInput;
    }
}
