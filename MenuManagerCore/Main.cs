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
    [JsonPropertyName("PisexMenusBridge")] public bool PisexMenusBridge { get; set; } = true;
    [JsonPropertyName("SoundScroll")] public string SoundScroll { get; set; } = "UI.StickerSelect";
    [JsonPropertyName("SoundClick")] public string SoundClick { get; set; } = "UI.StickerApply";
    [JsonPropertyName("SoundDisabled")] public string SoundDisabled { get; set; } = "Instructor.ImportantLessonStart";
    [JsonPropertyName("SoundBack")] public string SoundBack { get; set; } = "UI.StickerApply";
    [JsonPropertyName("SoundExit")] public string SoundExit { get; set; } = "EndMatch.ItemRevealSingle";
    [JsonPropertyName("HudOpenDelay")] public float HudOpenDelay { get; set; } = 0.2f;
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
    private readonly PluginCapability<IPmmPaintRegistry?> _paintCapability = new("pmm:paint");

    private CMenuApi? _api;
    private CCSGameRulesProxy? _gameRulesProxy;
    public override string ModuleName => "[FORK] MenuManager";
    public override string ModuleVersion => "v1.2.03";
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

        Control.Init(this);
        ModuleTranslations.Load(ModuleDirectory, Logger);
        RegisterListener<OnTick>(Control.OnPluginTick);
        RegisterListener<OnTick>(PanoramaHud.OnTick);
        CsgoMenu.Register(this);
        RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        RegisterEventHandler<EventPlayerTeam>(OnPlayerTeam);
        RegisterListener<OnMapStart>(OnMapStart);
        RegisterListener<OnCustomHudClicked>(PanoramaHud.OnClick);
        AddTimer(3.0f, PisexLayout.Sync, TimerFlags.REPEAT);
        RegisterEventHandler<EventPlayerDisconnect>((@event, _) =>
        {
            if (@event.Userid != null)
            {
                PanoramaHud.Close(eventPlayer: @event.Userid);
                CsgoMenu.Close(@event.Userid);
                PaintRegistry.Close(@event.Userid);
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

        if (Misc.GetCurrentPlayerMenu(player) != MenuType.CsgoMenu || !Misc.GetCsgoDeadHint(player))
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

        if (isButtonBased || currentType == MenuType.PanoramaWasdMenu)
        {
            _ = menu.AddMenuOption(Localizer["menumanager.sound_settings"], (p, _) => OpenSoundSettings(p));
        }

        if (currentType is MenuType.PanoramaMenu or MenuType.PanoramaWasdMenu)
        {
            _ = menu.AddMenuOption(Localizer["menumanager.panorama_settings"], (p, _) => OpenPanoramaSettings(p));
        }

        if (currentType == MenuType.CsgoMenu)
        {
            _ = menu.AddMenuOption(Localizer["menumanager.csgo_settings"], (p, _) => OpenCsgoSettings(p));
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
        _ = menu.AddMenuOption(Localizer["menumanager.csgo"], (p, _) => OpenCsgoHelp(p));
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

    private void OpenCsgoHelp(CCSPlayerController player)
    {
        if (_api == null)
        {
            return;
        }

        IMenu menu = _api.GetMenu(Localizer["menumanager.csgo"], OpenMenuTypeSettings);
        _ = menu.AddMenuOption(Localizer["menumanager.csgo_read_chat"], (_, _) => { }, true);
        _ = menu.AddMenuOption(Localizer["menumanager.csgo_copy_console"], (_, _) => { }, true);
        _ = menu.AddMenuOption(Localizer["menumanager.csgo_bind_cmd"], (_, _) => { }, true);
        _ = menu.AddMenuOption(Localizer["menumanager.csgo_dead"], (_, _) => { }, true);
        _ = menu.AddMenuOption(Localizer["menumanager.csgo_chat_keys"], (_, _) => { }, true);
        _ = menu.AddMenuOption(Localizer["menumanager.csgo_next"], (p, _) => OpenCsgoConfirm(p));
        menu.Open(player);
        PrintBinds(player, true, true);
    }

    private void OpenCsgoConfirm(CCSPlayerController player)
    {
        if (_api == null)
        {
            return;
        }

        IMenu menu = _api.GetMenu(Localizer["menumanager.csgo"], OpenCsgoHelp);
        _ = menu.AddMenuOption(Localizer["menumanager.csgo_warn_dead"], (_, _) => { }, true);
        _ = menu.AddMenuOption(Localizer["menumanager.csgo_dead_chat"], (_, _) => { }, true);
        _ = menu.AddMenuOption(Localizer["menumanager.csgo_binds_chat"], (p, _) => PrintBinds(p, true, false));
        _ = menu.AddMenuOption(Localizer["menumanager.csgo_binds_console"], (p, _) => PrintBinds(p, false, true));
        _ = menu.AddMenuOption(Localizer["menumanager.csgo_confirm"], (p, _) =>
        {
            Misc.SelectPlayerMenu(p, MenuType.CsgoMenu);
            OnCommand(p);
        });
        menu.Open(player);
    }

    private void PrintBinds(CCSPlayerController player, bool chat, bool console)
    {
        const string bindAll =
            "bind 1 \"slot1;css_1\"; bind 2 \"slot2;css_2\"; bind 3 \"slot3;css_3\"; bind 4 \"slot4;css_4\"; bind 5 \"slot5;css_5\"; bind 6 \"slot6;css_6\"; bind 7 \"slot7;css_7\"; bind 8 \"slot8;css_8\"; bind 9 \"slot9;css_9\"; bind 0 \"slot0;css_0\"";
        string dead = Localizer["menumanager.csgo_chat_dead"];
        string keys = Localizer["menumanager.csgo_chat_use"];
        Server.NextFrame(() =>
        {
            if (!player.IsValid)
            {
                return;
            }

            if (chat)
            {
                PrintTagged(player, dead);
                PrintTagged(player, keys);
                player.PrintToChat($" {bindAll}");
            }

            if (console)
            {
                player.PrintToConsole(dead);
                player.PrintToConsole(keys);
                player.PrintToConsole(bindAll);
            }
        });
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
        _ = menu.AddMenuOption(Localizer["menumanager.csgo_binds_chat"], (p, _) => PrintBinds(p, true, false));
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
