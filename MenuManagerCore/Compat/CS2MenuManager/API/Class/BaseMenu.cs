using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CS2MenuManager.API.Enum;
using CS2MenuManager.API.Interface;
using CS2MenuManager.Pmm;

namespace CS2MenuManager.API.Class;

/// <summary>
/// Base of every CS2MenuManager menu. Display always goes to PanoramaMenuManager,
/// so the player sees the menu type chosen in !menu (Panorama, WASD panorama, CSGO Menu, chat...).
/// </summary>
public abstract class BaseMenu(string title, BasePlugin plugin) : IMenu
{
    public string Title { get; set; } = title;
    public List<ItemOption> ItemOptions { get; internal set; } = [];
    public bool ExitButton { get; set; } = true;
    public int MenuTime { get; set; }
    public IMenu? PrevMenu { get; set; }
    public BasePlugin Plugin { get; } = plugin;

    public virtual ItemOption AddItem(string display, Action<CCSPlayerController, ItemOption> onSelect,
        DisableOption disableOption = DisableOption.None)
    {
        ItemOption option = new(display, disableOption, onSelect);
        ItemOptions.Add(option);
        return option;
    }

    public virtual ItemOption AddItem(string display, DisableOption disableOption)
    {
        ItemOption option = new(display, disableOption, null);
        ItemOptions.Add(option);
        return option;
    }

    public virtual void Display(CCSPlayerController player, int time)
    {
        PmmCompatBridge.Show(this, player, time);
    }

    // PanoramaMenuManager keeps its own page per title, so firstItem is not used.
    public virtual void DisplayAt(CCSPlayerController player, int firstItem, int time)
    {
        PmmCompatBridge.Show(this, player, time);
    }

    public void DisplayToAll(int time)
    {
        foreach (CCSPlayerController player in Utilities.GetPlayers().Where(p => !p.IsBot))
        {
            Display(player, time);
        }
    }

    public void DisplayAtToAll(int firstItem, int time)
    {
        foreach (CCSPlayerController player in Utilities.GetPlayers().Where(p => !p.IsBot))
        {
            DisplayAt(player, firstItem, time);
        }
    }

    // Look settings of the original menus. Plugins may set them; the panorama ignores them
    // (colors come from the client addon built in the constructor).
    public char ChatMenu_TitleColor { get; set; } = '\x10';
    public char ChatMenu_EnabledColor { get; set; } = '\x01';
    public char ChatMenu_DisabledColor { get; set; } = '\x08';
    public char ChatMenu_PrevPageColor { get; set; } = '\x10';
    public char ChatMenu_NextPageColor { get; set; } = '\x10';
    public char ChatMenu_ExitColor { get; set; } = '\x02';
    public string CenterHtmlMenu_TitleColor { get; set; } = "yellow";
    public string CenterHtmlMenu_EnabledColor { get; set; } = "white";
    public string CenterHtmlMenu_DisabledColor { get; set; } = "grey";
    public string CenterHtmlMenu_PrevPageColor { get; set; } = "yellow";
    public string CenterHtmlMenu_NextPageColor { get; set; } = "yellow";
    public string CenterHtmlMenu_ExitColor { get; set; } = "red";
    public bool CenterHtmlMenu_InlinePageOptions { get; set; } = true;
    public int CenterHtmlMenu_MaxTitleLength { get; set; }
    public int CenterHtmlMenu_MaxOptionLength { get; set; }
    public string WasdMenu_TitleColor { get; set; } = "green";
    public string WasdMenu_ScrollUpDownKeyColor { get; set; } = "cyan";
    public string WasdMenu_SelectKeyColor { get; set; } = "cyan";
    public string WasdMenu_PrevKeyColor { get; set; } = "cyan";
    public string WasdMenu_ExitKeyColor { get; set; } = "cyan";
    public string WasdMenu_SelectedOptionColor { get; set; } = "orange";
    public string WasdMenu_OptionColor { get; set; } = "white";
    public string WasdMenu_DisabledOptionColor { get; set; } = "grey";
    public string WasdMenu_ArrowColor { get; set; } = "purple";
    public bool WasdMenu_FreezePlayer { get; set; }
    public string WasdMenu_ScrollUpKey { get; set; } = "W";
    public string WasdMenu_ScrollDownKey { get; set; } = "S";
    public string WasdMenu_SelectKey { get; set; } = "E";
    public string WasdMenu_PrevKey { get; set; } = "Shift";
    public string WasdMenu_ExitKey { get; set; } = "R";
}
