using CounterStrikeSharp.API.Core;
using CS2MenuManager.API.Interface;

namespace CS2MenuManager.Pmm;

/// <summary>
/// What MenuManager.GetActiveMenu returns. Paging and input belong to PanoramaMenuManager,
/// so Page and CurrentOffset are always 0.
/// </summary>
public sealed class PmmMenuInstance(CCSPlayerController player, IMenu menu) : IMenuInstance
{
    public CCSPlayerController Player { get; } = player;
    public int Page => 0;
    public int CurrentOffset => 0;
    public int NumPerPage => 7;
    public Stack<int> PrevPageOffsets { get; } = new();
    public IMenu Menu { get; } = menu;

    public void NextPage()
    {
    }

    public void PrevPage()
    {
    }

    public void Reset()
    {
        Display();
    }

    public void Close(bool exitSound)
    {
        PmmCompatBridge.Close(Player);
    }

    public void Display()
    {
        PmmCompatBridge.Show(Menu, Player, Menu.MenuTime);
    }

    public void OnKeyPress(CCSPlayerController player, int key)
    {
        PmmCompatBridge.KeyPress(player, key);
    }

    public void Dispose()
    {
    }
}
