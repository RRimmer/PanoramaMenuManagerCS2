using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;

namespace MenuManager;

public interface IPmmMenuPainter
{
    bool TryPaint(CCSPlayerController player, IMenu menu);

    void Close(CCSPlayerController player);

    bool IsOpen(CCSPlayerController player);
}

public interface IPmmPaintRegistry
{
    void Add(IPmmMenuPainter painter);

    void Remove(IPmmMenuPainter painter);
}
