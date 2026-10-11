using CounterStrikeSharp.API.Core;

namespace MenuManager;

/// <summary>
/// Opens menus of plugins written for another menu library (CS2MenuManager by schwarper) in PanoramaMenuManager.
/// Capability "menu:nfcore:compat". MenuManagerCore implements it. The CS2MenuManager.dll that the core
/// writes to shared/CS2MenuManager calls it; ordinary plugins use IMenuApi instead.
/// </summary>
public interface IMenuCompatApi
{
    // Shows the menu in the player's PMM menu type. An empty menu is not opened.
    void Show(CCSPlayerController player, CompatMenu menu);

    // Closes the menu only if it was opened through Show.
    void Close(CCSPlayerController player);

    // Owner of the menu opened through Show that is still on screen, or null.
    object? GetActive(CCSPlayerController player);

    // Number-key input: 1..7 item, 8 back, 0 exit.
    void KeyPress(CCSPlayerController player, int key);
}

public sealed class CompatMenu(object owner)
{
    // The other library's menu object. GetActive returns it.
    public object Owner { get; } = owner;
    public string Title { get; set; } = "";
    public bool ExitButton { get; set; } = true;

    // Seconds until the menu closes by itself. 0 keeps it open.
    public int Time { get; set; }
    public List<CompatMenuItem> Items { get; } = [];

    // Back button. Null hides it.
    public Action<CCSPlayerController>? Back { get; set; }

    // Called for CompatPostSelect.Reset.
    public Action<CCSPlayerController>? Reopen { get; set; }
}

public sealed class CompatMenuItem(string text, bool disabled, Func<CCSPlayerController, CompatPostSelect>? onSelect)
{
    public string Text { get; set; } = text;
    public bool Disabled { get; set; } = disabled;

    // Runs the plugin callback and says what happens to the menu after it.
    // Ignored when the callback opened another menu.
    public Func<CCSPlayerController, CompatPostSelect>? OnSelect { get; set; } = onSelect;
}

public enum CompatPostSelect
{
    Close,
    Reset,
    Nothing
}
