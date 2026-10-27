using CounterStrikeSharp.API.Core;
using CS2MenuManager.API.Class;
using CS2MenuManager.API.Enum;

namespace CS2MenuManager.API.Interface;

// Same members as schwarper/CS2MenuManager 1.0.43.
public interface IMenu
{
    string Title { get; set; }
    List<ItemOption> ItemOptions { get; }
    bool ExitButton { get; set; }
    int MenuTime { get; set; }
    IMenu? PrevMenu { get; set; }
    BasePlugin Plugin { get; }

    ItemOption AddItem(string display, Action<CCSPlayerController, ItemOption> onSelect,
        DisableOption disableOption = DisableOption.None);

    ItemOption AddItem(string display, DisableOption disableOption);

    void Display(CCSPlayerController player, int time);
    void DisplayAt(CCSPlayerController player, int firstItem, int time);
    void DisplayToAll(int time);
    void DisplayAtToAll(int firstItem, int time);
}

public interface IMenuInstance : IDisposable
{
    CCSPlayerController Player { get; }
    int Page { get; }
    int CurrentOffset { get; }
    int NumPerPage { get; }
    Stack<int> PrevPageOffsets { get; }
    IMenu Menu { get; }

    void NextPage();
    void PrevPage();
    void Reset();
    void Close(bool exitSound);
    void Display();
    void OnKeyPress(CCSPlayerController player, int key);
}
