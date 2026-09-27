using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Extensions;
using CounterStrikeSharp.API.Modules.Menu;
using MenuManager;
using Microsoft.Extensions.Logging;

namespace MenuManagerCore;

internal static class PanoramaHud
{
    private const string LayoutPath = "panorama/layout/custom_game/menu_ui.xml";
    private const string DialogId = "utils-dialog";
    private const int ItemsPerPage = 6;
    private const int MaxSelectOptions = 12;
    private static readonly string[] ItemKinds = ["k-empty", "k-value", "k-submenu", "k-toggle", "k-select"];

    private static CCSCustomHudLayout? _layout;
    private static readonly SlotMenu[] Slots = new SlotMenu[64];
    private static readonly Dictionary<string, int>?[] PageMemory = new Dictionary<string, int>?[64];
    private static readonly int[] NoticeToken = new int[64];
    private static readonly string[] NoticeClasses = ["ntf-success", "ntf-warning", "ntf-error"];

    internal static void Reset()
    {
        _layout = null;
        Array.Clear(Slots);
        Array.Clear(PageMemory);
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

    internal static bool Show(CCSPlayerController player, MenuInstance menu)
    {
        if (!Misc.IsValidPlayer(player))
        {
            return false;
        }

        if (!EnsureLayout())
        {
            return false;
        }

        Control.CloseMenu(player);
        int slot = player.Slot;
        if (Slots[slot].Menu is { } open)
        {
            Remember(slot, open.Title, Slots[slot].Page);
        }

        int page = Recall(slot, menu.Title);
        Slots[slot] = new SlotMenu(menu, page);
        // Draw on later frames. Doing spawn/draw in the same tick as css_admin
        // stalls CCLCMsg_Move and Valve kicks with NETWORK_DISCONNECT_OVERFLOW.
        Server.NextFrame(() =>
        {
            if (!Misc.IsValidPlayer(player) || Slots[player.Slot].Menu == null)
            {
                return;
            }

            Draw(player, captureInput: false);
            Server.NextFrame(() =>
            {
                if (_layout is not { IsValid: true } || !Misc.IsValidPlayer(player) || Slots[player.Slot].Menu == null)
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
        if (_layout == null || !customLayout.IsValid || customLayout.Handle != _layout.Handle)
        {
            return;
        }

        if (!Misc.IsValidPlayer(player))
        {
            return;
        }

        int slot = player.Slot;
        SlotMenu state = Slots[slot];
        if (state.Menu == null)
        {
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
                    Slots[slot] = state;
                    Draw(player, captureInput: true);
                }

                return;
            case "utils-next":
                if (state.Page < PageCount(menu) - 1)
                {
                    state.Page++;
                    state.OpenSelect = -1;
                    Slots[slot] = state;
                    Draw(player, captureInput: true);
                }

                return;
            case "utils-back":
                if (state.Page > 0)
                {
                    state.Page--;
                    Slots[slot] = state;
                    Draw(player, captureInput: true);
                    return;
                }

                Hide(player, forgetPages: false);
                menu.BackAction?.Invoke(player);
                return;
        }

        if (buttonId.StartsWith("utils-opt-", StringComparison.Ordinal))
        {
            PickSelect(player, slot, state, menu, buttonId);
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

        ChatMenuOption option = menu.MenuOptions[index];
        if (!option.Disabled && MenuOptionKind.TryGetSelect(option, out MenuOptionKind.SelectState opened) &&
            opened.Choices.Length > 0 && opened.Choices.Length <= MaxSelectOptions)
        {
            state.OpenSelect = state.OpenSelect == visible ? -1 : visible;
            Slots[slot] = state;
            Draw(player, captureInput: true);
            return;
        }

        state.OpenSelect = -1;
        Slots[slot] = state;
        Remember(slot, menu.Title, state.Page);
        if (menu.PostSelectAction != PostSelectAction.Nothing)
        {
            Hide(player, forgetPages: false);
        }

        option.OnSelect(player, option);
        if (menu.PostSelectAction == PostSelectAction.Reset && menu.ResetAction != null)
        {
            Server.NextFrame(() => menu.ResetAction(player));
        }
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

        int slot = player.Slot;
        SlotMenu state = Slots[slot];
        MenuInstance? menu = state.Menu;
        if (menu == null)
        {
            Hide(player, forgetPages: true);
            return;
        }

        int pages = PageCount(menu);
        if (state.Page >= pages)
        {
            state.Page = pages - 1;
        }

        if (state.Page < 0)
        {
            state.Page = 0;
        }

        Slots[slot] = state;
        Remember(slot, menu.Title, state.Page);
        _layout.SetHasClassForPlayer(player, DialogId, "utils-dismissed", false);
        if (captureInput)
        {
            _layout.SetInputCaptureEnabled(player, true);
        }

        _layout.SetDialogVariableStringForPlayer(player, "utils-title", "title", Misc.ColorText(menu.Title, false));
        _layout.SetDialogVariableStringForPlayer(player, "utils-desc", "desc", "");
        _layout.SetDialogVariableStringForPlayer(player, "utils-page", "page", $"{state.Page + 1} / {pages}");
        _layout.SetHasClassForPlayer(player, "utils-back-anchor", "utils-hide", menu.BackAction == null && state.Page == 0);
        _layout.SetHasClassForPlayer(player, "utils-prev", "utils-disabled", state.Page <= 0);
        _layout.SetHasClassForPlayer(player, "utils-next", "utils-disabled", state.Page >= pages - 1);

        for (int i = 0; i < ItemsPerPage; i++)
        {
            string itemId = $"utils-item-{i}";
            int index = state.Page * ItemsPerPage + i;
            if (index >= menu.MenuOptions.Count)
            {
                SetKind(player, itemId, "k-empty");
                _layout.SetHasClassForPlayer(player, itemId, "is-on", false);
                _layout.SetHasClassForPlayer(player, itemId, "is-open", false);
                _layout.SetHasClassForPlayer(player, itemId, "is-up", false);
                SetSlotOpen(player, i, false);
                _layout.SetHasClassForPlayer(player, itemId, "is-disabled", false);
                _layout.SetDialogVariableStringForPlayer(player, itemId, "label", "");
                _layout.SetDialogVariableStringForPlayer(player, itemId, "value", "");
                FillSelect(player, i, null, "");
                continue;
            }

            ChatMenuOption option = menu.MenuOptions[index];
            string raw = option.Text ?? "";
            bool linked = raw.Contains(MenuOptionKind.ButtonMark);
            string visible = Misc.ColorText(raw, false).Replace(MenuOptionKind.ButtonMark.ToString(), "");
            string kind = "k-submenu";
            bool on = false;
            string label = visible;
            string value = "";
            MenuOptionKind.SelectState? select = null;
            if (MenuOptionKind.TryGetToggle(option, out on))
            {
                kind = "k-toggle";
            }
            else if (MenuOptionKind.TryGetSelect(option, out MenuOptionKind.SelectState found) &&
                     found.Choices.Length > 0 && found.Choices.Length <= MaxSelectOptions)
            {
                kind = "k-select";
                select = found;
                value = found.Value;
            }
            else if (!linked && MenuOptionKind.TryParseToggleLabel(visible, out string parsed, out on))
            {
                kind = "k-toggle";
                label = parsed;
            }

            bool open = kind == "k-select" && state.OpenSelect == i;
            SetKind(player, itemId, kind);
            _layout.SetHasClassForPlayer(player, itemId, "is-on", kind == "k-toggle" && on);
            _layout.SetHasClassForPlayer(player, itemId, "is-open", open);
            _layout.SetHasClassForPlayer(player, itemId, "is-up", open && i >= 3);
            SetSlotOpen(player, i, open);
            _layout.SetHasClassForPlayer(player, itemId, "is-disabled", option.Disabled);
            _layout.SetDialogVariableStringForPlayer(player, itemId, "label", label);
            _layout.SetDialogVariableStringForPlayer(player, itemId, "value", value);
            FillSelect(player, i, select, value);
        }
    }

    private static void PickSelect(CCSPlayerController player, int slot, SlotMenu state, MenuInstance menu, string buttonId)
    {
        string[] parts = buttonId.Split('-');
        if (parts.Length != 4 ||
            !int.TryParse(parts[2], out int visible) ||
            !int.TryParse(parts[3], out int choice))
        {
            return;
        }

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

        state.OpenSelect = -1;
        Slots[slot] = state;
        Remember(slot, menu.Title, state.Page);
        select.OnSelect?.Invoke(player, option, choice);
        if (Slots[slot].Menu == menu)
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
            string label = shown ? select!.Choices[choice] : "";
            _layout.SetHasClassForPlayer(player, optionId, "utils-sel", shown &&
                label.Equals(current, StringComparison.OrdinalIgnoreCase));
            _layout.SetDialogVariableStringForPlayer(player, $"utils-opt-label-{visible}-{choice}", $"o{visible}_{choice}", label);
        }
    }

    private static void Remember(int slot, string title, int page)
    {
        if (slot is < 0 or >= 64 || string.IsNullOrEmpty(title))
        {
            return;
        }

        Dictionary<string, int> pages = PageMemory[slot] ??= new Dictionary<string, int>(StringComparer.Ordinal);
        pages[title] = page;
    }

    private static int Recall(int slot, string title)
    {
        if (slot is < 0 or >= 64 || string.IsNullOrEmpty(title))
        {
            return 0;
        }

        Dictionary<string, int>? pages = PageMemory[slot];
        return pages != null && pages.TryGetValue(title, out int page) ? page : 0;
    }

    private static void Forget(int slot)
    {
        if (slot is >= 0 and < 64)
        {
            PageMemory[slot] = null;
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

            Slots[player.Slot] = default;
        }

        if (_layout is not { IsValid: true })
        {
            return;
        }

        _layout.SetHasClassForPlayer(player, DialogId, "utils-dismissed", true);
        _layout.SetInputCaptureEnabled(player, false);
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

    private static int PageCount(MenuInstance menu)
    {
        int count = Math.Max(1, menu.MenuOptions.Count);
        return (count + ItemsPerPage - 1) / ItemsPerPage;
    }

    private struct SlotMenu(MenuInstance? menu, int page)
    {
        public MenuInstance? Menu = menu;
        public int Page = page;
        public int OpenSelect = -1;
    }
}
