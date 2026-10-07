using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using MenuManager;

namespace MenuManagerCore;

internal static class PaintRegistry
{
    private static readonly List<IPmmMenuPainter> Painters = [];

    internal static void Add(IPmmMenuPainter painter)
    {
        if (!Painters.Contains(painter))
        {
            Painters.Add(painter);
        }
    }

    internal static void Remove(IPmmMenuPainter painter)
    {
        _ = Painters.Remove(painter);
    }

    internal static bool TryPaint(CCSPlayerController player, IMenu menu)
    {
        foreach (IPmmMenuPainter painter in Painters.ToArray())
        {
            if (painter.TryPaint(player, menu))
            {
                return true;
            }
        }

        return false;
    }

    internal static void Close(CCSPlayerController player)
    {
        foreach (IPmmMenuPainter painter in Painters.ToArray())
        {
            painter.Close(player);
        }
    }

    internal static bool IsOpen(CCSPlayerController player)
    {
        foreach (IPmmMenuPainter painter in Painters)
        {
            if (painter.IsOpen(player))
            {
                return true;
            }
        }

        return false;
    }
}

internal sealed class PaintRegistryApi : IPmmPaintRegistry
{
    public void Add(IPmmMenuPainter painter)
    {
        PaintRegistry.Add(painter);
    }

    public void Remove(IPmmMenuPainter painter)
    {
        PaintRegistry.Remove(painter);
    }
}
