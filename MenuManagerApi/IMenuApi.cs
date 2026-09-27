using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;

namespace MenuManager;

public interface IMenuApi
{
    public IMenu GetMenu(string title, Action<CCSPlayerController>? backAction = null,
        Action<CCSPlayerController>? resetAction = null);

    // Deprecated, only for backward compatibility
    [Obsolete("Method used only for backward compatibility, not for develope.", true)]
    public IMenu NewMenu(string title, Action<CCSPlayerController>? backAction = null);
    //

    public IMenu GetMenuForcetype(string title, MenuType type, Action<CCSPlayerController>? backAction = null,
        Action<CCSPlayerController>? resetAction = null);

    // Deprecated, only for backward compatibility
    [Obsolete("Method used only for backward compatibility, not for develope.", true)]
    public IMenu NewMenuForcetype(string title, MenuType type, Action<CCSPlayerController>? backAction = null);
    // 

    public void CloseMenu(CCSPlayerController player);
    public MenuType GetMenuType(CCSPlayerController player);
    public bool HasOpenedMenu(CCSPlayerController player);

    public ChatMenuOption AddToggle(IMenu menu, string label, bool on,
        Action<CCSPlayerController, ChatMenuOption> onSelect, bool disabled = false);

    public ChatMenuOption AddSelect(IMenu menu, string label, string value, string[] choices,
        Action<CCSPlayerController, ChatMenuOption, int> onSelect, bool disabled = false);

    public void Notify(CCSPlayerController player, string title, string text, MenuNotice notice = MenuNotice.Success);
}

public enum MenuNotice
{
    Success,
    Warning,
    Error
}

public enum MenuType
{
    Default = -1,
    ChatMenu = 0,
    ConsoleMenu = 1,
    CenterMenu = 2,
    ButtonMenu = 3,
    MetamodMenu = 4,
    PanoramaMenu = 5
}
