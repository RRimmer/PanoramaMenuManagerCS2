using MenuManager;

namespace MenuManagerCore;

public class PlayerSettings
{
    public MenuType MenuType { get; set; } = MenuType.Default;
    public bool MenuChosen { get; set; }
    public bool? UsePagination { get; set; }
    public bool? SoundsEnabled { get; set; }
    public float? Volume { get; set; }
    public bool? Notifications { get; set; }
    public string? MenuPosition { get; set; }
    public bool? CsgoDeadHint { get; set; }
}
