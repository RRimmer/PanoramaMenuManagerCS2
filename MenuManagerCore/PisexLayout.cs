using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Extensions;

namespace MenuManagerCore;

// Pisex cs2-menus (HUD_LAYOUT) spawns its own custom_hud_layout with the same menu_ui.xml.
// It marks an open dropdown on utils-item-N and writes option text to {s:value} of utils-opt-N-M,
// while this layout opens the list from utils-slot-N and reads {s:oN_M}.
internal static class PisexLayout
{
    private const int Rows = 6;
    private static readonly List<CCSCustomHudLayout> Layouts = [];
    private static readonly string?[] Applied = new string?[64];

    internal static void Reset()
    {
        Layouts.Clear();
        Array.Clear(Applied);
    }

    internal static void Sync()
    {
        if (!Enabled())
        {
            return;
        }

        bool changed = Layouts.RemoveAll(layout => !layout.IsValid) > 0;
        foreach (CCSCustomHudLayout layout in Utilities.FindAllEntitiesByDesignerName<CCSCustomHudLayout>("custom_hud_layout"))
        {
            changed |= Track(layout);
        }

        if (changed)
        {
            Array.Clear(Applied);
        }

        if (Layouts.Count == 0)
        {
            return;
        }

        foreach (CCSPlayerController player in Misc.GetValidPlayers())
        {
            string key = LookKey(player);
            if (player.Slot is < 0 or >= 64 || Applied[player.Slot] == key)
            {
                continue;
            }

            Applied[player.Slot] = key;
            foreach (CCSCustomHudLayout layout in Layouts)
            {
                PanoramaHud.ApplyLook(layout, player);
            }
        }
    }

    internal static void Refresh(CCSPlayerController player)
    {
        if (!Enabled() || player.Slot is < 0 or >= 64)
        {
            return;
        }

        Applied[player.Slot] = LookKey(player);
        foreach (CCSCustomHudLayout layout in Layouts)
        {
            if (layout.IsValid)
            {
                PanoramaHud.ApplyLook(layout, player);
            }
        }
    }

    internal static void OnClick(CCSPlayerController player, CCSCustomHudLayout layout)
    {
        if (!Enabled() || !Misc.IsValidPlayer(player) || !IsForeign(layout))
        {
            return;
        }

        _ = Track(layout);
        PanoramaHud.ApplyLook(layout, player);
        Server.NextFrame(() => Mirror(player, layout));
    }

    private static bool Enabled()
    {
        return Control.GetPlugin()?.Config.PisexMenusBridge == true;
    }

    private static bool Track(CCSCustomHudLayout layout)
    {
        if (!IsForeign(layout) || Layouts.Exists(known => known.Handle == layout.Handle))
        {
            return false;
        }

        Layouts.Add(layout);
        return true;
    }

    private static bool IsForeign(CCSCustomHudLayout layout)
    {
        return layout.IsValid && !PanoramaHud.IsOwnLayout(layout) &&
               string.Equals(layout.StrLayout, PanoramaHud.LayoutPath, StringComparison.OrdinalIgnoreCase);
    }

    private static string LookKey(CCSPlayerController player)
    {
        return Misc.GetPlayerPosition(player) ?? "";
    }

    private static void Mirror(CCSPlayerController player, CCSCustomHudLayout layout)
    {
        if (!layout.IsValid || !Misc.IsValidPlayer(player))
        {
            return;
        }

        CCSCustomHudLayoutState? state = null;
        foreach (CCSCustomHudLayoutState candidate in layout.PlayerLayoutStates)
        {
            if (candidate.PlayerSlot == player.Slot)
            {
                state = candidate;
                break;
            }
        }

        if (state == null)
        {
            return;
        }

        int dialog = -1;
        Dictionary<int, int> items = [];
        Dictionary<int, (int Row, int Column)> options = [];
        int index = 0;
        foreach (string panel in layout.PanelIds)
        {
            if (panel == "utils-dialog")
            {
                dialog = index;
            }
            else if (TryParseCell(panel, "utils-item-", out int row, out _, 1))
            {
                items[index] = row;
            }
            else if (TryParseCell(panel, "utils-opt-", out row, out int column, 2))
            {
                options[index] = (row, column);
            }

            index++;
        }

        int isOpen = IndexOf(layout.ClassNames, "is-open");
        int isUp = IndexOf(layout.ClassNames, "is-up");
        int dismissed = IndexOf(layout.ClassNames, "utils-dismissed");
        bool[] open = new bool[Rows];
        bool[] up = new bool[Rows];
        bool hidden = false;
        foreach (HUDPanelHasClass_t entry in state.HasClasses)
        {
            if (entry.ClassStatus != EHudPanelClassStatus_t.k_eHudPanelClassStatus_HasClass)
            {
                continue;
            }

            int panel = entry.PanelIdIndex;
            int name = entry.ClassNameIndex;
            if (panel == dialog && name == dismissed)
            {
                hidden = true;
            }
            else if (items.TryGetValue(panel, out int row))
            {
                open[row] |= name == isOpen;
                up[row] |= name == isUp;
            }
        }

        List<(int Row, int Column, string Text)> labels = [];
        int value = IndexOf(layout.DialogVariableNames, "value");
        if (value >= 0 && !hidden)
        {
            foreach (HUDPanelDialogVariableString_t entry in state.DialogVariableStrings)
            {
                if (entry.DialogVariableIndex == value &&
                    options.TryGetValue(entry.PanelIdIndex, out (int Row, int Column) cell) && open[cell.Row])
                {
                    labels.Add((cell.Row, cell.Column, entry.IsSet ? entry.Value : ""));
                }
            }
        }

        for (int row = 0; row < Rows; row++)
        {
            bool show = open[row] && !hidden;
            layout.SetHasClassForPlayer(player, $"utils-slot-{row}", "is-open", show);
            layout.SetHasClassForPlayer(player, $"utils-slot-{row}", "is-up", show && up[row]);
        }

        foreach ((int row, int column, string text) in labels)
        {
            layout.SetDialogVariableStringForPlayer(player, $"utils-opt-label-{row}-{column}", $"o{row}_{column}", text);
        }
    }

    private static int IndexOf(NetworkedVector<string> names, string name)
    {
        int index = 0;
        foreach (string item in names)
        {
            if (item == name)
            {
                return index;
            }

            index++;
        }

        return -1;
    }

    private static bool TryParseCell(string panel, string prefix, out int row, out int column, int parts)
    {
        row = -1;
        column = -1;
        if (!panel.StartsWith(prefix, StringComparison.Ordinal))
        {
            return false;
        }

        string[] numbers = panel[prefix.Length..].Split('-');
        if (numbers.Length != parts || !int.TryParse(numbers[0], out row) || row is < 0 or >= Rows)
        {
            return false;
        }

        return parts == 1 || int.TryParse(numbers[1], out column);
    }
}
