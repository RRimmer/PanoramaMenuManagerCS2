using MenuManager;

namespace MenuManagerCore;

public class PlayerSettings
{
    public MenuType MenuType { get; set; } = MenuType.ButtonMenu;
    public bool? UsePagination { get; set; }
    public bool? SoundsEnabled { get; set; }
    public float? Volume { get; set; }
    public bool? Notifications { get; set; }
}