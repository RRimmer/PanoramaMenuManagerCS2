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

    // Panorama layout file of the PMM HUD (custom_hud_layout), for example
    // "panorama/layout/custom_game/my_menu.xml". The file must keep the ids and classes of menu_ui.xml.
    // null or empty restores the default. One layout for the whole server: the last call wins.
    // Open panorama menus are closed and the HUD is spawned again with the new file.
    public void SetPanoramaHudLayout(string path);
    public string GetPanoramaHudLayout();

    public IMenu GetMenuForcetype(string title, MenuType type, Action<CCSPlayerController>? backAction = null,
        Action<CCSPlayerController>? resetAction = null);

    // Deprecated, only for backward compatibility
    [Obsolete("Method used only for backward compatibility, not for develope.", true)]
    public IMenu NewMenuForcetype(string title, MenuType type, Action<CCSPlayerController>? backAction = null);
    // 

    public void CloseMenu(CCSPlayerController player);
    public MenuType GetMenuType(CCSPlayerController player);

    // The selected type, including WASD panorama. GetMenuType reports WASD as PanoramaMenu.
    public MenuType GetSelectedMenu(CCSPlayerController player);
    public bool HasOpenedMenu(CCSPlayerController player);

    public ChatMenuOption AddToggle(IMenu menu, string label, bool on,
        Action<CCSPlayerController, ChatMenuOption> onSelect, bool disabled = false);

    public ChatMenuOption AddSelect(IMenu menu, string label, string value, string[] choices,
        Action<CCSPlayerController, ChatMenuOption, int> onSelect, bool disabled = false);

    public void Notify(CCSPlayerController player, string title, string text, MenuNotice notice = MenuNotice.Success);

    // ---- Number keys (1.3.00) ----
    // CSGO Menu and PanoramaDigitMenu are driven by the player's number keys. The player binds them once:
    // GetNumberKeysBindLine() = spec_usenumberkeys_nobinds 0;bind "1" "css_1; slot1";...;bind "0" "css_0; slot10".
    // The server cannot write binds into the client (CS2 blocks bind/alias from the server).

    // The line to paste into the client console.
    public string GetNumberKeysBindLine();

    // Prints the line with a short how-to to the player's chat and console.
    public void PrintNumberKeysBinds(CCSPlayerController player);

    // True after the player passed the number keys check (saved in the database).
    public bool HasNumberKeysChecked(CCSPlayerController player);

    // Opens the number keys check on the CSGO-style panel: 1..0 light up as the server receives them.
    // onDone(player, true) when all ten came in and the player pressed 1 ("Done");
    // onDone(player, false) on exit, timeout or when another menu opened.
    public void OpenNumberKeysCheck(CCSPlayerController player, Action<CCSPlayerController, bool>? onDone = null);

    // Every css_0 ... css_9 the server receives (digit 0..9), also when no menu is open.
    // Raised before the core uses the key for its own menu.
    public event Action<CCSPlayerController, int>? NumberKeyPressed;

    // ---- Menu sounds (1.3.00) ----
    // The sounds from MenuManagerCore.json (SoundScroll, SoundClick, SoundDisabled, SoundBack, SoundNext, SoundExit),
    // so a plugin with its own panel sounds like the core menus. Own sounds come from the workshop addon
    // (soundevents_addon.vsndevts, "PrecacheResources" in the config).

    // Plays the sound to the player. Respects the player's sound toggle in !menu and "SoundVolume".
    public void PlayMenuSound(CCSPlayerController player, MenuSound sound);

    // The sound event name from the config ("" = none). Next falls back to Scroll when SoundNext is empty.
    public string GetMenuSound(MenuSound sound);
}

public enum MenuSound
{
    Scroll,
    Click,
    Disabled,
    Back,
    Next,
    Exit
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
    PanoramaMenu = 5,
    PanoramaWasdMenu = 6,
    CsgoMenu = 7,
    // The panorama panel without the mouse: items 1-6, 8 back, 9 next, 0 exit (number keys).
    // GetMenuType reports it as PanoramaMenu, GetSelectedMenu as PanoramaDigitMenu.
    PanoramaDigitMenu = 8
}
