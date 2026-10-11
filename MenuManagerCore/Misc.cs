using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using MenuManager;
using Microsoft.Extensions.Logging;

namespace MenuManagerCore;

internal static class Misc
{
    private static string _defaultMenu = "ButtonMenu";
    private static MenuType _defaultMenuType = MenuType.ButtonMenu;

    public static List<CCSPlayerController> GetValidPlayers()
    {
        List<CCSPlayerController> players = [];
        foreach (CCSPlayerController player in Utilities.GetPlayers())
        {
            if (player is
                { IsValid: true, IsBot: false, IsHLTV: false, Connected: PlayerConnectedState.Connected })
            {
                players.Add(player);
            }
        }

        return players;
    }

    public static void SetDefaultMenu(string defaultMenuStr)
    {
        List<string> menuTypes = new(["ButtonMenu", "CenterMenu", "ConsoleMenu", "ChatMenu", "MetamodMenu", "PanoramaMenu", "PanoramaWasdMenu", "CsgoMenu"]);
        if (menuTypes.Contains(defaultMenuStr))
        {
            _defaultMenu = defaultMenuStr;
            if (Enum.TryParse(defaultMenuStr, out MenuType result))
            {
                _defaultMenuType = result;
            }
        }
        else
        {
            Control.GetPlugin()
                ?.Logger
                .LogInformation(
                    "Invalid menu type: {DefaultMenuStr}. Using default menu {DefaultMenu}", defaultMenuStr,
                    _defaultMenu);
        }
    }

    private static PlayerSettings GetOrAddSettings(ulong steamId)
    {
        if (MenuManagerCore.PlayerSettingsCache.TryGetValue(steamId, out PlayerSettings? value))
        {
            return value;
        }

        value = new PlayerSettings();
        MenuManagerCore.PlayerSettingsCache[steamId] = value;
        return value;
    }

    public static MenuType GetCurrentPlayerMenu(CCSPlayerController player)
    {
        if (!IsValidPlayer(player))
        {
            return _defaultMenuType;
        }

        ulong? steamId = player.AuthorizedSteamID?.SteamId64;
        if (steamId == null ||
            !MenuManagerCore.PlayerSettingsCache.TryGetValue(steamId.Value, out PlayerSettings? settings))
        {
            return _defaultMenuType;
        }

        if (settings.MenuChosen)
        {
            return settings.MenuType == MenuType.Default ? _defaultMenuType : settings.MenuType;
        }

        return settings.MenuType is MenuType.Default or MenuType.ButtonMenu
            ? _defaultMenuType
            : settings.MenuType;
    }

    public static bool GetPlayerPagination(CCSPlayerController player)
    {
        return !IsValidPlayer(player) || player.AuthorizedSteamID == null
            ? Control.GetPlugin()!.Config.Pagination
            : MenuManagerCore.PlayerSettingsCache.TryGetValue(player.AuthorizedSteamID.SteamId64, out PlayerSettings? settings)
            ? settings.UsePagination ?? Control.GetPlugin()!.Config.Pagination
            : Control.GetPlugin()!.Config.Pagination;
    }

    public static bool GetPlayerNotifications(CCSPlayerController player)
    {
        if (Control.GetPlugin()?.Config.Notifications != true)
        {
            return false;
        }

        if (!IsValidPlayer(player) || player.AuthorizedSteamID == null)
        {
            return true;
        }

        return !MenuManagerCore.PlayerSettingsCache.TryGetValue(player.AuthorizedSteamID.SteamId64, out PlayerSettings? settings) ||
               settings.Notifications != false;
    }

    public static bool GetCsgoDeadHint(CCSPlayerController player)
    {
        if (!IsValidPlayer(player) || player.AuthorizedSteamID == null)
        {
            return true;
        }

        return !MenuManagerCore.PlayerSettingsCache.TryGetValue(player.AuthorizedSteamID.SteamId64, out PlayerSettings? settings) ||
               settings.CsgoDeadHint != false;
    }

    public static void SetCsgoDeadHint(CCSPlayerController player, bool enabled)
    {
        if (!IsValidPlayer(player) || player.AuthorizedSteamID == null)
        {
            return;
        }

        ulong steamId = player.AuthorizedSteamID.SteamId64;
        PlayerSettings settings = GetOrAddSettings(steamId);
        settings.CsgoDeadHint = enabled;
        MenuManagerCore.PlayerSettingsCache[steamId] = settings;
        SaveSettingsAsync(steamId, settings);
    }

    public static void SetPlayerNotifications(CCSPlayerController player, bool enabled)
    {
        if (!IsValidPlayer(player) || player.AuthorizedSteamID == null)
        {
            return;
        }

        ulong steamId = player.AuthorizedSteamID.SteamId64;
        PlayerSettings settings = GetOrAddSettings(steamId);
        settings.Notifications = enabled;
        MenuManagerCore.PlayerSettingsCache[steamId] = settings;
        SaveSettingsAsync(steamId, settings);
    }

    internal static string? NormalizePosition(string? value)
    {
        return value?.Trim().ToLowerInvariant() switch
        {
            "left" => "left",
            "center" or "centre" or "middle" => "center",
            "right" => "right",
            _ => null
        };
    }

    public static string? GetPlayerPosition(CCSPlayerController player)
    {
        return GetOwnPosition(player) ?? NormalizePosition(Control.GetPlugin()?.Config.PanoramaPosition);
    }

    public static string? GetOwnPosition(CCSPlayerController player)
    {
        if (!IsValidPlayer(player) || player.AuthorizedSteamID == null)
        {
            return null;
        }

        return MenuManagerCore.PlayerSettingsCache.TryGetValue(player.AuthorizedSteamID.SteamId64, out PlayerSettings? settings)
            ? settings.MenuPosition
            : null;
    }

    public static void SetPlayerPosition(CCSPlayerController player, string? position)
    {
        if (!IsValidPlayer(player) || player.AuthorizedSteamID == null)
        {
            return;
        }

        ulong steamId = player.AuthorizedSteamID.SteamId64;
        PlayerSettings settings = GetOrAddSettings(steamId);
        settings.MenuPosition = NormalizePosition(position);
        MenuManagerCore.PlayerSettingsCache[steamId] = settings;
        SaveSettingsAsync(steamId, settings);
    }

    public static bool GetPlayerSoundsEnabled(CCSPlayerController player)
    {
        return !IsValidPlayer(player) || player.AuthorizedSteamID == null || !MenuManagerCore.PlayerSettingsCache.TryGetValue(player.AuthorizedSteamID.SteamId64, out PlayerSettings? settings) || (settings.SoundsEnabled ?? true);
    }

    public static float GetPlayerVolume(CCSPlayerController player)
    {
        float configVol = Control.GetPlugin()!.Config.SoundVolume;
        return !IsValidPlayer(player) || player.AuthorizedSteamID == null
            ? configVol
            : MenuManagerCore.PlayerSettingsCache.TryGetValue(player.AuthorizedSteamID.SteamId64, out PlayerSettings? settings)
            ? settings.Volume ?? configVol
            : configVol;
    }

    public static void SelectPlayerMenu(CCSPlayerController player, MenuType type)
    {
        if (!IsValidPlayer(player) || player.AuthorizedSteamID == null)
        {
            return;
        }

        ulong steamId = player.AuthorizedSteamID.SteamId64;

        PlayerSettings settings = GetOrAddSettings(steamId);
        settings.MenuType = type;
        settings.MenuChosen = true;

        MenuManagerCore.PlayerSettingsCache[steamId] = settings;

        player.PrintToChat($"{Control.GetPlugin()?.Localizer["menumanager.selected_type"]} {GetMenuTypeName(type)}");
        SaveSettingsAsync(steamId, settings);
        CounterStrikeSharp.API.Modules.Menu.MenuManager.CloseActiveMenu(player);
        PanoramaHud.CloseMenu(player);
        CsgoMenu.Close(player);
    }

    public static void SetPlayerPagination(CCSPlayerController player, bool pagination)
    {
        if (!IsValidPlayer(player) || player.AuthorizedSteamID == null)
        {
            return;
        }

        ulong steamId = player.AuthorizedSteamID.SteamId64;

        PlayerSettings settings = GetOrAddSettings(steamId);
        settings.UsePagination = pagination;
        MenuManagerCore.PlayerSettingsCache[steamId] = settings;

        SaveSettingsAsync(steamId, settings);
    }

    public static void SetPlayerSoundsEnabled(CCSPlayerController player, bool enabled)
    {
        if (!IsValidPlayer(player) || player.AuthorizedSteamID == null)
        {
            return;
        }

        ulong steamId = player.AuthorizedSteamID.SteamId64;

        PlayerSettings settings = GetOrAddSettings(steamId);
        settings.SoundsEnabled = enabled;
        MenuManagerCore.PlayerSettingsCache[steamId] = settings;

        SaveSettingsAsync(steamId, settings);
    }

    public static void SetPlayerVolume(CCSPlayerController player, float volume)
    {
        if (!IsValidPlayer(player) || player.AuthorizedSteamID == null)
        {
            return;
        }

        ulong steamId = player.AuthorizedSteamID.SteamId64;

        volume = Math.Clamp(volume, 0.0f, 0.9f);

        PlayerSettings settings = GetOrAddSettings(steamId);
        settings.Volume = volume;
        MenuManagerCore.PlayerSettingsCache[steamId] = settings;

        SaveSettingsAsync(steamId, settings);
    }

    private static void SaveSettingsAsync(ulong steamId, PlayerSettings settings)
    {
        _ = Task.Run(async () =>
        {
            if (MenuManagerCore.DataBaseService != null)
            {
                await MenuManagerCore.DataBaseService.SaveMenuSetting(steamId, settings);
            }
        });
    }

    private static string GetMenuTypeName(MenuType type)
    {
        return type switch
        {
            MenuType.ChatMenu => Control.GetPlugin()?.Localizer["menumanager.chat"] ??
                                 throw new InvalidOperationException(),
            MenuType.ConsoleMenu => Control.GetPlugin()?.Localizer["menumanager.console"] ??
                                    throw new InvalidOperationException(),
            MenuType.CenterMenu => Control.GetPlugin()?.Localizer["menumanager.center"] ??
                                   throw new InvalidOperationException(),
            MenuType.ButtonMenu => Control.GetPlugin()?.Localizer["menumanager.control"] ??
                                   throw new InvalidOperationException(),
            MenuType.MetamodMenu => Control.GetPlugin()?.Localizer["menumanager.metamod"] ??
                                    throw new InvalidOperationException(),
            MenuType.PanoramaMenu => Control.GetPlugin()?.Localizer["menumanager.panorama_mouse"] ??
                                     throw new InvalidOperationException(),
            MenuType.PanoramaWasdMenu => Control.GetPlugin()?.Localizer["menumanager.panorama_wasd"] ??
                                         throw new InvalidOperationException(),
            MenuType.CsgoMenu => Control.GetPlugin()?.Localizer["menumanager.csgo"] ??
                                 throw new InvalidOperationException(),
            _ => "Undefined"
        };
    }

    public static bool IsValidPlayer(CCSPlayerController player)
    {
        return player is
        { IsValid: true, Connected: PlayerConnectedState.Connected, IsBot: false };
    }

    internal static string ColorText(string text, bool needColors = true)
    {
        string newText = text;
        List<string> colors = new([
            "Default", "White", "Darkred", "Green", "Lightyellow", "Lightblue", "Olive", "Lime", "Red", "Lightpurple",
            "Purple", "Grey", "Yellow", "Gold", "Silver", "Blue", "Darkblue", "Bluegrey", "Magenta", "Lightred",
            "Orange"
        ]);

        if (needColors)
        {
            foreach (string color0 in colors)
            {
                string color = "[color:" + color0 + "]";
                string colorOld = "{" + color0 + "}";
                string rep = $"<font color='{color0.ToLower()}'>";
                newText = newText.Replace(color, rep, StringComparison.CurrentCultureIgnoreCase);
                newText = newText.Replace(colorOld, rep, StringComparison.CurrentCultureIgnoreCase);
            }
        }
        else
        {
            foreach (string color0 in colors)
            {
                string color = "[color:" + color0 + "]";
                string colorOld = "{" + color0 + "}";
                newText = newText.Replace(color, "", StringComparison.CurrentCultureIgnoreCase);
                newText = newText.Replace(colorOld, "", StringComparison.CurrentCultureIgnoreCase);
            }

            newText = newText.Replace("</font>", "");
        }

        return newText;
    }
}
