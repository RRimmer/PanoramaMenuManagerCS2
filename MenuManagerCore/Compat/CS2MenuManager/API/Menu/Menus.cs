using CounterStrikeSharp.API.Core;
using CS2MenuManager.API.Class;

namespace CS2MenuManager.API.Menu;

// All menu classes of CS2MenuManager open the same PanoramaMenuManager menu.
// The look is the player's choice in !menu, not the class the plugin picked.

public class PlayerMenu(string title, BasePlugin plugin) : BaseMenu(title, plugin)
{
}

public class ChatMenu(string title, BasePlugin plugin) : BaseMenu(title, plugin)
{
}

public class ConsoleMenu(string title, BasePlugin plugin) : BaseMenu(title, plugin)
{
}

public class CenterHtmlMenu(string title, BasePlugin plugin) : BaseMenu(title, plugin)
{
}

public class WasdMenu(string title, BasePlugin plugin) : BaseMenu(title, plugin)
{
}
