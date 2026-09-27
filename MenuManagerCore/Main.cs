using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Menu;
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
    [JsonPropertyName("SoundScroll")] public string SoundScroll { get; set; } = "UI.StickerSelect";
    [JsonPropertyName("SoundClick")] public string SoundClick { get; set; } = "UI.StickerApply";
    [JsonPropertyName("SoundDisabled")] public string SoundDisabled { get; set; } = "Instructor.ImportantLessonStart";
    [JsonPropertyName("SoundBack")] public string SoundBack { get; set; } = "UI.StickerApply";
    [JsonPropertyName("SoundExit")] public string SoundExit { get; set; } = "EndMatch.ItemRevealSingle";
    [JsonPropertyName("StopingUser")] public bool StopingUser { get; set; } = true;
    [JsonPropertyName("ButtonsConfig")] public ButtonsConfig ButtonsConfig { get; set; } = new();
    [JsonPropertyName("IgnoreErrors")] public bool IgnoreErrors { get; set; } = true;
    [JsonPropertyName("MenuLinesCount")] public int MenuLinesCount { get; set; } = 5;
    [JsonPropertyName("UseMetamodMenu")] public bool UseMetamodMenu { get; set; } = false;
    [JsonPropertyName("UseMetamodMenuReplace")] public bool UseMetamodMenuReplace { get; set; } = false;
}

public class MenuManagerCore : BasePlugin, IPluginConfig<PluginConfig>
{
    internal static DataBaseService? DataBaseService;
    internal static ConcurrentDictionary<ulong, PlayerSettings> PlayerSettingsCache = new();
    private readonly PluginCapability<IMenuApi?> _pluginCapability = new("menu:nfcore");

    private CMenuApi? _api;
    private CCSGameRulesProxy? _gameRulesProxy;
    public override string ModuleName => "[FORK] MenuManager";
    public override string ModuleVersion => "v1.1.10";
    public override string ModuleAuthor => "E!N (base by Nick Fox)";
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

        Misc.SetDefaultMenu(Config.DefaultMenu);

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

        Control.Init(this);
        RegisterListener<OnTick>(Control.OnPluginTick);
        RegisterListener<OnMapStart>(OnMapStart);
        RegisterListener<OnCustomHudClicked>(PanoramaHud.OnClick);
        RegisterEventHandler<EventPlayerDisconnect>((@event, _) =>
        {
            if (@event.Userid != null)
            {
                PanoramaHud.Close(eventPlayer: @event.Userid);
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

    private void OnMapStart(string map)
    {
        if (Config.MenuFlashFix)
        {
            _gameRulesProxy = null;
        }

        PanoramaHud.Reset();
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

            _ = menu.AddMenuOption(Localizer["menumanager.sound_settings"], (p, _) => OpenSoundSettings(p));
        }

        if (currentType == MenuType.PanoramaMenu)
        {
            _ = menu.AddMenuOption(Localizer["menumanager.panorama_settings"], (p, _) => OpenPanoramaSettings(p));
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

        _ = menu.AddMenuOption(Localizer["menumanager.console"], (p, _) => Misc.SelectPlayerMenu(p, MenuType.ConsoleMenu));
        _ = menu.AddMenuOption(Localizer["menumanager.chat"], (p, _) => Misc.SelectPlayerMenu(p, MenuType.ChatMenu));
        _ = menu.AddMenuOption(Localizer["menumanager.center"], (p, _) => Misc.SelectPlayerMenu(p, MenuType.CenterMenu));
        _ = menu.AddMenuOption(Localizer["menumanager.control"], (p, _) => Misc.SelectPlayerMenu(p, MenuType.ButtonMenu));

        if (Config is { UseMetamodMenu: true, UseMetamodMenuReplace: false })
        {
            _ = menu.AddMenuOption(Localizer["menumanager.metamod"],
                (p, _) => Misc.SelectPlayerMenu(p, MenuType.MetamodMenu));
        }

        _ = menu.AddMenuOption("Panorama", (p, _) => Misc.SelectPlayerMenu(p, MenuType.PanoramaMenu));

        menu.Open(player);
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

    private void OpenSoundSettings(CCSPlayerController player)
    {
        if (_api == null)
        {
            return;
        }

        IMenu menu = _api.GetMenu(Localizer["menumanager.sound_settings"], OnCommand);

        bool areSoundsEnabled = Misc.GetPlayerSoundsEnabled(player);
        string toggleText = areSoundsEnabled
            ? Localizer["menumanager.disable_sounds"]
            : Localizer["menumanager.enable_sounds"];

        _ = menu.AddMenuOption(toggleText, (p, _) =>
        {
            Misc.SetPlayerSoundsEnabled(p, !areSoundsEnabled);
            OpenSoundSettings(p);
        });

        if (areSoundsEnabled)
        {
            _ = menu.AddMenuOption(Localizer["menumanager.volume_settings"], (p, _) => OpenVolumeSettings(p));
        }

        menu.Open(player);
    }

    private void OpenVolumeSettings(CCSPlayerController player)
    {
        if (_api == null)
        {
            return;
        }

        float currentVol = Misc.GetPlayerVolume(player);
        IMenu menu = _api.GetMenu($"{Localizer["menumanager.volume"]}: {currentVol:0.0}", OpenSoundSettings);

        _ = menu.AddMenuOption(Localizer["menumanager.volume_up"], (p, _) =>
        {
            Misc.SetPlayerVolume(p, currentVol + 0.1f);
            OpenVolumeSettings(p);
        }, currentVol >= 0.9f);

        _ = menu.AddMenuOption(Localizer["menumanager.volume_down"], (p, _) =>
        {
            Misc.SetPlayerVolume(p, currentVol - 0.1f);
            OpenVolumeSettings(p);
        }, currentVol <= 0.0f);

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

        menu.Open(player);
    }
}
