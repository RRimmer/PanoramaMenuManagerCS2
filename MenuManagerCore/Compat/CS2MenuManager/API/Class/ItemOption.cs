using CounterStrikeSharp.API.Core;
using CS2MenuManager.API.Enum;

namespace CS2MenuManager.API.Class;

public class ItemOption(string display, DisableOption option, Action<CCSPlayerController, ItemOption>? onSelect)
{
    public string Text { get; set; } = display;
    public DisableOption DisableOption { get; set; } = option;

    // Close by default, as in CS2MenuManager. The callback may switch it to Nothing
    // (cs2-store does that for Inspect and for "not enough credits").
    public PostSelectAction PostSelectAction { get; set; } = PostSelectAction.Close;
    public Action<CCSPlayerController, ItemOption>? OnSelect { get; set; } = onSelect;
}
