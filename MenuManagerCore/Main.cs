using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using MenuManager;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using static CounterStrikeSharp.API.Core.Listeners;

namespace MenuManagerCore;

public class PluginConfig : BasePluginConfig
{
    [JsonPropertyName("DatabaseHost")] public string DatabaseHost { get; set; } = "";
    [JsonPropertyName("DatabasePort")] public int DatabasePort { get; set; } = 3306;
    [JsonPropertyName("DatabaseUser")] public string DatabaseUser { get; set; } = "";
    [JsonPropertyName("DatabasePassword")] public string DatabasePassword { get; set; } = "";
    [JsonPropertyName("DatabaseName")] public string DatabaseName { get; set; } = "";
    [JsonPropertyName("DefaultMenu")] public string DefaultMenu { get; set; } = "PanoramaMenu";
    [JsonPropertyName("MenuFlashFix")] public bool MenuFlashFix { get; set; } = false;
    [JsonPropertyName("TrimMenuOptions")] public bool TrimMenuOptions { get; set; } = true;
    [JsonPropertyName("TrimSelectedOption")] public int TrimSelectedOption { get; set; } = 24;
    [JsonPropertyName("TrimNonSelectedOption")] public int TrimNonSelectedOption { get; set; } = 28;
    [JsonPropertyName("MenuSettingsCommand")] public List<string> MenuSettingsCommand { get; set; } = ["menu", "menus"];
    [JsonPropertyName("MenuAddon")] public int MenuAddon { get; set; } = 0;
    [JsonPropertyName("MenuAddonUrl")] public string MenuAddonUrl { get; set; } = "pisex.online/module/buttons";
    [JsonPropertyName("MenuTime")] public float MenuTime { get; set; } = 120.0f;
    [JsonPropertyName("Pagination")] public bool Pagination { get; set; } = false;
    [JsonPropertyName("SoundVolume")] public float SoundVolume { get; set; } = 0.25f;
    [JsonPropertyName("Notifications")] public bool Notifications { get; set; } = true;
    [JsonPropertyName("PanoramaPosition")] public string PanoramaPosition { get; set; } = "";
    // Empty = panorama/layout/custom_game/menu_ui.xml. Another file must keep the ids and classes of menu_ui.xml.
    [JsonPropertyName("PanoramaLayoutPath")] public string PanoramaLayoutPath { get; set; } = "";
    [JsonPropertyName("PisexMenusBridge")] public bool PisexMenusBridge { get; set; } = true;
    [JsonPropertyName("SoundScroll")] public string SoundScroll { get; set; } = "UI.StickerSelect";
    [JsonPropertyName("SoundClick")] public string SoundClick { get; set; } = "UI.StickerApply";
    [JsonPropertyName("SoundDisabled")] public string SoundDisabled { get; set; } = "Instructor.ImportantLessonStart";
    [JsonPropertyName("SoundBack")] public string SoundBack { get; set; } = "UI.StickerApply";
    // Next page (CSGO Menu 9, digits panorama 9). Empty = SoundScroll as before.
    [JsonPropertyName("SoundNext")] public string SoundNext { get; set; } = "";
    [JsonPropertyName("SoundExit")] public string SoundExit { get; set; } = "EndMatch.ItemRevealSingle";
    [JsonPropertyName("HudOpenDelay")] public float HudOpenDelay { get; set; } = 0.2f;
    [JsonPropertyName("StopingUser")] public bool StopingUser { get; set; } = true;
    [JsonPropertyName("ButtonsConfig")] public ButtonsConfig ButtonsConfig { get; set; } = new();
    [JsonPropertyName("IgnoreErrors")] public bool IgnoreErrors { get; set; } = true;
    [JsonPropertyName("MenuLinesCount")] public int MenuLinesCount { get; set; } = 5;
    [JsonPropertyName("UseMetamodMenu")] public bool UseMetamodMenu { get; set; } = false;
    [JsonPropertyName("UseMetamodMenuReplace")] public bool UseMetamodMenuReplace { get; set; } = false;

    // Menus of plugins built for schwarper/CS2MenuManager (cs2-store) through PMM.
    // The core writes shared/CS2MenuManager/CS2MenuManager.dll on start.
    [JsonPropertyName("CS2MenuManagerCompat")] public bool CS2MenuManagerCompat { get; set; } = true;

    // Number keys check (CSGO Menu, Panorama digits, IMenuApi.OpenNumberKeysCheck): seconds without a key before it ends.
    [JsonPropertyName("NumberKeysTestTimeout")] public float NumberKeysTestTimeout { get; set; } = 40f;

    // Files the server precaches on map load, for example "soundevents/soundevents_addon.vsndevts"
    // (own menu sounds from the workshop addon; then put the event names into Sound* above).
    [JsonPropertyName("PrecacheResources")] public List<string> PrecacheResources { get; set; } = [];
}

public class MenuManagerCore : BasePlugin, IPluginConfig<PluginConfig>
{
    internal static DataBaseService? DataBaseService;
    internal static ConcurrentDictionary<ulong, PlayerSettings> PlayerSettingsCache = new();
    private readonly PluginCapability<IMenuApi?> _pluginCapability = new("menu:nfcore");
    private readonly PluginCapability<IPmmPaintRegistry?> _paintCapability = new("pmm:paint");
    private readonly PluginCapability<IMenuCompatApi?> _compatCapability = new("menu:nfcore:compat");

    private CMenuApi? _api;
    private MenuCompatApi? _compat;
    private CCSGameRulesProxy? _gameRulesProxy;
    public override string ModuleName => "[FORK] MenuManager";
    public override string ModuleVersion => "v1.3.00";
    public override string ModuleAuthor => "Rimmer (base by Nick Fox)";
    public override string ModuleDescription => "";
    public required PluginConfig Config { get; set; }

    public void OnConfigParsed(PluginConfig config)
    {
        Config = config;
        if (Config.MenuLinesCount is > 5 or < 1)
        {
            Config.MenuLinesCount = 5;
            Logger.LogWarning("MenuLinesCount is incorrect. Setting it to default (= 5)");
        }

        if (Config.HudOpenDelay <= 0f)
        {
            Config.HudOpenDelay = 0.01f;
        }

        Misc.SetDefaultMenu(Config.DefaultMenu);
        PanoramaHud.SetLayoutPath(Config.PanoramaLayoutPath, "config");

        DataBaseService = new DataBaseService(config, ModuleDirectory);

        _ = Task.Run(async () =>
        {
            await DataBaseService.TestAndCheckDataBaseTableAsync();
            PlayerSettingsCache = await DataBaseService.LoadAllMenuSettings();
            Logger.LogInformation("Database initialized and cache loaded.");
        });
    }

    public override void Load(bool hotReload)
    {
        _api = new CMenuApi();
        Capabilities.RegisterPluginCapability(_pluginCapability, () => _api);
        Capabilities.RegisterPluginCapability(_paintCapability, () => new PaintRegistryApi());
        _compat = new MenuCompatApi(_api);
        Capabilities.RegisterPluginCapability(_compatCapability, () => _compat);
        if (Config.CS2MenuManagerCompat)
        {
            Cs2MenuManagerDeploy.Install(this);
        }

        Control.Init(this);
        ModuleTranslations.Load(ModuleDirectory, Logger);
        RegisterListener<OnTick>(Control.OnPluginTick);
        RegisterListener<OnTick>(PanoramaHud.OnTick);
        CsgoMenu.Register(this);
        NumberKeys.Register(this);
        RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        RegisterEventHandler<EventPlayerTeam>(OnPlayerTeam);
        RegisterListener<OnMapStart>(OnMapStart);
        RegisterListener<OnServerPrecacheResources>(manifest =>
        {
            foreach (string resource in Config.PrecacheResources)
            {
                if (!string.IsNullOrWhiteSpace(resource))
                {
                    manifest.AddResource(resource.Trim());
                }
            }
        });
        RegisterListener<OnCustomHudClicked>(PanoramaHud.OnClick);
        AddTimer(3.0f, PisexLayout.Sync, TimerFlags.REPEAT);
        RegisterEventHandler<EventPlayerDisconnect>((@event, _) =>
        {
            if (@event.Userid != null)
            {
                PanoramaHud.Close(eventPlayer: @event.Userid);
                CsgoMenu.Close(@event.Userid);
                PaintRegistry.Close(@event.Userid);
                _compat?.Forget(@event.Userid.Slot);
                NumberKeys.Forget(@event.Userid.Slot);
                if (Config.UseMetamodMenu)
                {
                    MenusMm.ClearCallbackInfo(@event.Userid.Slot);
                }
            }

            return HookResult.Continue;
        }, HookMode.Pre);
        Config.MenuSettingsCommand.ForEach(c =>
        {
            AddCommand($"css_{c}", "Menu settings", (player, _) => OnCommand(player));
        });
        if (hotReload)
        {
            MenusMm.Init();
        }
    }

    private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo _)
    {
        CCSPlayerController? player = @event.Userid;
        if (player != null)
        {
            RemindCsgo(player, true);
        }

        return HookResult.Continue;
    }

    private HookResult OnPlayerTeam(EventPlayerTeam @event, GameEventInfo _)
    {
        CCSPlayerController? player = @event.Userid;
        if (player != null && !@event.Disconnect &&
            @event.Team == (int)CsTeam.Spectator &&
            @event.Oldteam != (int)CsTeam.Spectator)
        {
            RemindCsgo(player, false);
        }

        return HookResult.Continue;
    }

    private void RemindCsgo(CCSPlayerController player, bool died)
    {
        if (!player.IsValid || player.IsBot || player.Connected != PlayerConnectedState.Connected)
        {
            return;
        }

        if (Misc.GetCurrentPlayerMenu(player) is not (MenuType.CsgoMenu or MenuType.PanoramaDigitMenu) ||
            !Misc.GetCsgoDeadHint(player))
        {
            return;
        }

        PrintTagged(player, Localizer[died ? "menumanager.csgo_died" : "menumanager.csgo_spec"]);
        PrintTagged(player, Localizer["menumanager.csgo_hint_off"]);
    }

    private static void PrintTagged(CCSPlayerController player, string text)
    {
        player.PrintToChat($" {ChatColors.Red}[MENU] {ChatColors.Green}{text}");
    }

    private void OnMapStart(string map)
    {
        if (Config.MenuFlashFix)
        {
            _gameRulesProxy = null;
        }

        PanoramaHud.Reset();
        CsgoMenu.Reset();
        NumberKeys.Reset();
        PisexLayout.Reset();
        PanoramaHud.Warmup();
        MenusMm.Init();
    }

    private CCSGameRulesProxy? GetGameRulesProxy()
    {
        if (_gameRulesProxy == null || !_gameRulesProxy.IsValid)
        {
            _gameRulesProxy =
                Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").FirstOrDefault();
        }

        return _gameRulesProxy;
    }

    public void UpdateGameRulesRestartState()
    {
        CCSGameRulesProxy? proxy = GetGameRulesProxy();

        if (proxy == null || !proxy.IsValid || proxy.GameRules == null)
        {
            return;
        }

        CCSGameRules gameRules = proxy.GameRules;

        if (gameRules.WarmupPeriod)
        {
            return;
        }

        bool expectedState = gameRules.RestartRoundTime < Server.CurrentTime;

        if (gameRules.GameRestart != expectedState)
        {
            gameRules.GameRestart = expectedState;
            Utilities.SetStateChanged(proxy, "CCSGameRulesProxy", "m_pGameRules");
        }
    }

    public override void Unload(bool hotReload)
    {
        Control.Clear();
        PanoramaHud.ReleaseAll();
        Cs2MenuManagerDeploy.Uninstall();
    }

    private void OnCommand(CCSPlayerController? player)
    {
        if (player == null || _api == null)
        {
            return;
        }

        IMenu menu = _api.GetMenu(Localizer["menumanager.settings_title"]);

        _ = menu.AddMenuOption(Localizer["menumanager.select_type"], (p, _) => OpenMenuTypeSettings(p));

        MenuType currentType = Misc.GetCurrentPlayerMenu(player);
        bool isButtonBased = currentType is MenuType.ButtonMenu or MenuType.MetamodMenu;

        if (isButtonBased)
        {
            _ = menu.AddMenuOption(Localizer["menumanager.nav_mode"], (p, _) => OpenNavigationSettings(p));
        }

        // The types that play the menu sounds (SoundClick, SoundBack, SoundNext, SoundExit ...): on/off right here.
        if (isButtonBased || currentType is MenuType.PanoramaWasdMenu or MenuType.PanoramaDigitMenu or MenuType.CsgoMenu)
        {
            _ = _api.AddToggle(menu, Localizer["menumanager.menu_sound"], Misc.GetPlayerSoundsEnabled(player), (p, _) =>
            {
                Misc.SetPlayerSoundsEnabled(p, !Misc.GetPlayerSoundsEnabled(p));
                OnCommand(p);
            });
        }

        if (currentType is MenuType.PanoramaMenu or MenuType.PanoramaWasdMenu or MenuType.PanoramaDigitMenu)
        {
            _ = menu.AddMenuOption(Localizer["menumanager.panorama_settings"], (p, _) => OpenPanoramaSettings(p));
        }

        if (currentType == MenuType.CsgoMenu)
        {
            _ = menu.AddMenuOption(Localizer["menumanager.csgo_settings"], (p, _) => OpenCsgoSettings(p));
        }

        if (currentType is MenuType.CsgoMenu or MenuType.PanoramaDigitMenu)
        {
            _ = menu.AddMenuOption(Localizer["menumanager.keys_test"],
                (p, _) => NumberKeys.Start(p, null, (q, ok) => { if (ok) OnCommand(q); }, OnCommand));
        }

        menu.Open(player);
    }

    private void OpenMenuTypeSettings(CCSPlayerController player)
    {
        if (_api == null)
        {
            return;
        }

        IMenu menu = _api.GetMenu(Localizer["menumanager.select_type"], OnCommand);

        _ = menu.AddMenuOption(Localizer["menumanager.panorama_wasd"],
            (p, _) => Misc.SelectPlayerMenu(p, MenuType.PanoramaWasdMenu));
        _ = menu.AddMenuOption(Localizer["menumanager.panorama_mouse"],
            (p, _) => Misc.SelectPlayerMenu(p, MenuType.PanoramaMenu));
        // The number-key menus open the keys check first; 1 after all ten keys turns the menu on.
        _ = menu.AddMenuOption(Localizer["menumanager.panorama_digits"],
            (p, _) => StartTypeCheck(p, MenuType.PanoramaDigitMenu));
        _ = menu.AddMenuOption(Localizer["menumanager.csgo"], (p, _) => StartTypeCheck(p, MenuType.CsgoMenu));
        _ = menu.AddMenuOption(Localizer["menumanager.center"], (p, _) => Misc.SelectPlayerMenu(p, MenuType.CenterMenu));
        _ = menu.AddMenuOption(Localizer["menumanager.control"], (p, _) => Misc.SelectPlayerMenu(p, MenuType.ButtonMenu));
        _ = menu.AddMenuOption(Localizer["menumanager.chat"], (p, _) => Misc.SelectPlayerMenu(p, MenuType.ChatMenu));

        if (Config is { UseMetamodMenu: true, UseMetamodMenuReplace: false })
        {
            _ = menu.AddMenuOption(Localizer["menumanager.metamod"],
                (p, _) => Misc.SelectPlayerMenu(p, MenuType.MetamodMenu));
        }

        menu.Open(player);
    }

    private void StartTypeCheck(CCSPlayerController player, MenuType type)
    {
        NumberKeys.Start(player, type, (p, ok) =>
        {
            if (ok)
            {
                OnCommand(p);
            }
        }, OpenMenuTypeSettings);
    }

    private void OpenCsgoSettings(CCSPlayerController player)
    {
        if (_api == null)
        {
            return;
        }

        IMenu menu = _api.GetMenu(Localizer["menumanager.csgo_settings"], OnCommand);
        AddPositionSelect(player, menu, OpenCsgoSettings);
        bool hint = Misc.GetCsgoDeadHint(player);
        _ = _api.AddToggle(menu, Localizer["menumanager.csgo_hint"], hint, (p, _) =>
        {
            Misc.SetCsgoDeadHint(p, !Misc.GetCsgoDeadHint(p));
            OpenCsgoSettings(p);
        });
        _ = menu.AddMenuOption(Localizer["menumanager.csgo_binds_chat"], (p, _) => NumberKeys.PrintBinds(p));
        menu.Open(player);
    }

    private void AddPositionSelect(CCSPlayerController player, IMenu menu, Action<CCSPlayerController> reopen)
    {
        string?[] positions = [null, "left", "center", "right"];
        string[] labels =
        [
            Localizer["menumanager.position_default"], Localizer["menumanager.position_left"],
            Localizer["menumanager.position_center"], Localizer["menumanager.position_right"]
        ];
        int current = Math.Max(0, Array.IndexOf(positions, Misc.GetOwnPosition(player)));
        _ = _api!.AddSelect(menu, Localizer["menumanager.position"], labels[current], labels,
            (p, _, index) =>
            {
                Misc.SetPlayerPosition(p, positions[index]);
                PisexLayout.Refresh(p);
                reopen(p);
            });
    }

    private void OpenNavigationSettings(CCSPlayerController player)
    {
        if (_api == null)
        {
            return;
        }

        bool currentPagination = Misc.GetPlayerPagination(player);
        string statusText = currentPagination ? Localizer["menumanager.pagination"] : Localizer["menumanager.scroll"];

        IMenu menu = _api.GetMenu($"{Localizer["menumanager.nav_mode"]}: {statusText}", OnCommand);

        _ = menu.AddMenuOption($"{Localizer["menumanager.pagination"]}", (p, _) =>
        {
            Misc.SetPlayerPagination(p, true);
            OpenNavigationSettings(p);
        }, currentPagination);

        _ = menu.AddMenuOption($"{Localizer["menumanager.scroll"]}", (p, _) =>
        {
            Misc.SetPlayerPagination(p, false);
            OpenNavigationSettings(p);
        }, !currentPagination);

        menu.Open(player);
    }

    private void OpenPanoramaSettings(CCSPlayerController player)
    {
        if (_api == null)
        {
            return;
        }

        IMenu menu = _api.GetMenu(Localizer["menumanager.panorama_settings"], OnCommand);
        bool enabled = Misc.GetPlayerNotifications(player);
        if (!Config.Notifications)
        {
            _ = menu.AddMenuOption(Localizer["menumanager.notifications_server_off"], (_, _) => { }, true);
        }
        else
        {
            _ = _api.AddToggle(menu, Localizer["menumanager.notifications"], enabled, (p, _) =>
            {
                Misc.SetPlayerNotifications(p, !Misc.GetPlayerNotifications(p));
                OpenPanoramaSettings(p);
            });
        }

        AddPositionSelect(player, menu, OpenPanoramaSettings);

        menu.Open(player);
    }
}
