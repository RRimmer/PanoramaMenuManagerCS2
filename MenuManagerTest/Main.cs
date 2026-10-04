using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using MenuManager;

public class MenuManagerTest : BasePlugin
{
    private readonly PluginCapability<IMenuApi?> _pluginCapability = new("menu:nfcore");

    private IMenuApi? _api;
    private bool _toggle = true;
    private int _color;
    private int _map;
    public override string ModuleName => "MenuManager [Test]";
    public override string ModuleVersion => "1.2.00";
    public override string ModuleAuthor => "Rimmer (base by Nick Fox)";
    public override string ModuleDescription => "MenuManager Test Module";

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        _api = _pluginCapability.Get();
        if (_api == null)
        {
            Console.WriteLine("MenuManager Core not found...");
        }
    }

    [ConsoleCommand("mm_test", "Test menu")]
    public void OnCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || _api == null)
        {
            return;
        }

        Open(player);
    }

    private void Open(CCSPlayerController player)
    {
        IMenu menu = _api!.GetMenu("MenuManager 1.2.00");
        string[] colors = ["Red", "Green", "Blue", "Gold"];
        string[] maps =
        [
            "de_dust2", "de_mirage", "de_inferno", "de_nuke", "de_overpass", "de_vertigo",
            "de_ancient", "de_anubis", "de_train", "de_cache", "cs_office", "cs_italy", "de_cbble"
        ];

        _ = _api.AddToggle(menu, "Notifications", _toggle, (p, _) =>
        {
            _toggle = !_toggle;
            _api.Notify(p, "Toggle", _toggle ? "On" : "Off",
                _toggle ? MenuNotice.Success : MenuNotice.Warning);
            Open(p);
        });

        _ = _api.AddSelect(menu, "Color", colors[_color], colors, (p, _, index) =>
        {
            _color = index;
            _api.Notify(p, "Color", colors[index]);
            Open(p);
        });

        _ = _api.AddSelect(menu, "Map", maps[_map], maps, (p, _, index) =>
        {
            _map = index;
            _api.Notify(p, "Map", maps[index]);
            Open(p);
        });

        for (int i = 1; i <= 8; i++)
        {
            int n = i;
            _ = menu.AddMenuOption($"Button {n}", (p, option) => p.PrintToChat($"Selected: {option.Text}"));
        }

        menu.Open(player);
    }
}
