using CounterStrikeSharp.API.Core;
using CS2MenuManager.API.Interface;
using CS2MenuManager.API.Menu;
using CS2MenuManager.Pmm;

namespace CS2MenuManager.API.Class;

public static class MenuManager
{
    public static readonly Dictionary<string, Type> MenuTypesList = new()
    {
        { "ConsoleMenu", typeof(ConsoleMenu) },
        { "ChatMenu", typeof(ChatMenu) },
        { "WasdMenu", typeof(WasdMenu) },
        { "CenterHtmlMenu", typeof(CenterHtmlMenu) }
    };

    public static IMenuInstance? GetActiveMenu(CCSPlayerController player)
    {
        return PmmCompatBridge.GetActive(player);
    }

    // Closes only a menu this layer opened. Menus of other plugins stay open.
    public static void CloseActiveMenu(CCSPlayerController player)
    {
        PmmCompatBridge.Close(player);
    }

    public static void OpenMenu<TMenu>(CCSPlayerController player, TMenu menu, int? firstItem,
        Func<CCSPlayerController, TMenu, IMenuInstance> createInstance)
        where TMenu : IMenu
    {
        PmmCompatBridge.Show(menu, player, menu.MenuTime);
    }

    public static void OnKeyPress(CCSPlayerController player, int key)
    {
        PmmCompatBridge.KeyPress(player, key);
    }

    public static T CreateMenu<T>(string title, BasePlugin plugin) where T : BaseMenu
    {
        return (T)MenuByType(typeof(T), title, plugin);
    }

    public static BaseMenu MenuByType(Type menuType, string title, BasePlugin plugin)
    {
        if (menuType == typeof(PlayerMenu)) return new PlayerMenu(title, plugin);
        if (menuType == typeof(ChatMenu)) return new ChatMenu(title, plugin);
        if (menuType == typeof(ConsoleMenu)) return new ConsoleMenu(title, plugin);
        if (menuType == typeof(CenterHtmlMenu)) return new CenterHtmlMenu(title, plugin);
        if (menuType == typeof(WasdMenu)) return new WasdMenu(title, plugin);

        // A plugin's own BaseMenu subclass with the (title, plugin) constructor.
        if (typeof(BaseMenu).IsAssignableFrom(menuType) && !menuType.IsAbstract &&
            Activator.CreateInstance(menuType, title, plugin) is BaseMenu custom)
        {
            return custom;
        }

        throw new ArgumentException($"Unsupported menu type: {menuType.FullName}", nameof(menuType));
    }

    // Any name works: the menu is drawn by PanoramaMenuManager anyway.
    // cs2-store passes its config value "MenuType" here.
    public static BaseMenu MenuByType(string menuType, string title, BasePlugin plugin)
    {
        return menuType switch
        {
            "ChatMenu" => new ChatMenu(title, plugin),
            "ConsoleMenu" => new ConsoleMenu(title, plugin),
            "CenterHtmlMenu" => new CenterHtmlMenu(title, plugin),
            "WasdMenu" => new WasdMenu(title, plugin),
            _ => new PlayerMenu(title, plugin)
        };
    }

    // CS2MenuManager.toml is not used. Look and menu type live in MenuManagerCore.
    public static void ReloadConfig()
    {
    }
}
