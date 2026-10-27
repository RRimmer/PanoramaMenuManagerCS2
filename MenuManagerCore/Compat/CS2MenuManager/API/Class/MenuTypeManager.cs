using CounterStrikeSharp.API.Core;
using CS2MenuManager.API.Menu;

namespace CS2MenuManager.API.Class;

// The player picks the menu type in PanoramaMenuManager (!menu). These calls stay
// so plugins built against CS2MenuManager still load; they always report PlayerMenu.
public static class MenuTypeManager
{
    public static Type GetDefaultMenu()
    {
        return typeof(PlayerMenu);
    }

    public static Type? GetPlayerMenuType(CCSPlayerController player)
    {
        return typeof(PlayerMenu);
    }

    public static Type SetPlayerMenuType(CCSPlayerController player, Type menuType)
    {
        return menuType;
    }
}
