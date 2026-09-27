using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using MenuManager;

namespace MenuManagerCore;

internal class CMenuApi : IMenuApi
{
    public IMenu GetMenu(string title, Action<CCSPlayerController>? backAction = null,
        Action<CCSPlayerController>? resetAction = null)
    {
        return new MenuInstance(title, backAction, resetAction);
    }

    public IMenu NewMenu(string title, Action<CCSPlayerController>? backAction = null)
    {
        return new MenuInstance(title, backAction);
    }

    public IMenu GetMenuForcetype(string title, MenuType type, Action<CCSPlayerController>? backAction = null,
        Action<CCSPlayerController>? resetAction = null)
    {
        return new MenuInstance(title, backAction, resetAction, type);
    }

    public IMenu NewMenuForcetype(string title, MenuType type, Action<CCSPlayerController>? backAction = null)
    {
        return new MenuInstance(title, backAction, null, type);
    }

    public void CloseMenu(CCSPlayerController player)
    {
        Control.CloseMenu(player);
    }

    public MenuType GetMenuType(CCSPlayerController player)
    {
        return Misc.GetCurrentPlayerMenu(player);
    }

    public bool HasOpenedMenu(CCSPlayerController player)
    {
        return Control.HasOpenedMenu(player);
    }

    public ChatMenuOption AddToggle(IMenu menu, string label, bool on,
        Action<CCSPlayerController, ChatMenuOption> onSelect, bool disabled = false)
    {
        ChatMenuOption option = menu.AddMenuOption(label, onSelect, disabled);
        MenuOptionKind.SetToggle(option, on);
        return option;
    }

    public ChatMenuOption AddSelect(IMenu menu, string label, string value, string[] choices,
        Action<CCSPlayerController, ChatMenuOption, int> onSelect, bool disabled = false)
    {
        ChatMenuOption option = menu.AddMenuOption(label, (_, _) => { }, disabled);
        MenuOptionKind.SetSelect(option, value, choices, onSelect);
        return option;
    }

    public void Notify(CCSPlayerController player, string title, string text, MenuNotice notice = MenuNotice.Success)
    {
        PanoramaHud.Notify(player, title, text, notice);
    }
}