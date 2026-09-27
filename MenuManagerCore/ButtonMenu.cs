using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;

namespace MenuManagerCore;

public class ButtonMenu(string title, bool metamod = false) : IMenu
{
    public Action<CCSPlayerController>? BackAction = null;
    public bool Metamod = metamod;
    public Action<CCSPlayerController>? ResetAction = null;

    public string Title { get; set; } = title;

    public List<ChatMenuOption> MenuOptions { get; } = [];

    public bool ExitButton { get; set; }

    public PostSelectAction PostSelectAction { get; set; } = PostSelectAction.Nothing;

    public ChatMenuOption AddMenuOption(string display, Action<CCSPlayerController, ChatMenuOption> onSelect,
        bool disabled = false)
    {
        ChatMenuOption option = new(display, disabled, onSelect);
        MenuOptions.Add(option);
        return option;
    }

    public void Open(CCSPlayerController player)
    {
        Control.AddMenu(player, this);
    }

    public void OpenToAll()
    {
        Control.AddMenuAll(this);
    }
}