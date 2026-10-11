using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;

namespace MenuManagerCore;

internal static class Control
{
    private static readonly List<PlayerInfo> Menus = [];
    private static MenuManagerCore? _hPlugin;

    public static void AddMenu(CCSPlayerController player, ButtonMenu inst)
    {
        int oldSelected = 0;
        string oldTitle = "";
        int oldOffset = 0;
        for (int i = 0; i < Menus.Count; i++)
        {
            if (Menus[i].GetPlayer() == player)
            {
                oldSelected = Menus[i].Selected();
                oldTitle = Menus[i].Menu.Title;
                oldOffset = Menus[i].Offset();
                _ = Menus.Remove(Menus[i]);
                i++;
            }
        }

        PlayerInfo menu = new(player, inst, oldSelected, oldOffset, oldTitle);
        Menus.Add(menu);
    }

    public static void AddMenuAll(ButtonMenu inst)
    {
        List<CCSPlayerController> players = Utilities.GetPlayers();
        foreach (CCSPlayerController? player in players.OfType<CCSPlayerController?>().Where(player =>
                     player is
                     {
                         IsValid: true, IsBot: false, IsHLTV: false, Connected: PlayerConnectedState.Connected
                     }))
        {
            if (player != null)
            {
                AddMenu(player, inst);
            }
        }
    }

    public static void Clear()
    {
        foreach (CCSPlayerController? player in Menus.Select(menu => menu.GetPlayer()))
        {
            if (Misc.IsValidPlayer(player) &&
                _hPlugin is { Config.StopingUser: true } &&
                player.PawnIsAlive &&
                player.Pawn.Value != null &&
                player.Pawn.Value.MoveType == MoveType_t.MOVETYPE_NONE)
            {
                player.Pawn.Value.MoveType = MoveType_t.MOVETYPE_WALK;
                Schema.SetSchemaValue(player.Pawn.Value.Handle, "CBaseEntity", "m_nActualMoveType", 2);
                Utilities.SetStateChanged(player.Pawn.Value, "CBaseEntity", "m_MoveType");
            }
        }

        _ = Menus.RemoveAll(_ => true);
    }

    public static void OnPluginTick()
    {
        if (_hPlugin is { Config.MenuFlashFix: true })
        {
            _hPlugin.UpdateGameRulesRestartState();
        }

        if (Menus.Count <= 0)
        {
            return;
        }

        for (int i = 0; i < Menus.Count; i++)
        {
            PlayerInfo menu = Menus[i];
            CCSPlayerController player = menu.GetPlayer();
            if (!Misc.IsValidPlayer(player))
            {
                Menus.RemoveAt(i);
                i--;
                continue;
            }

            if (_hPlugin != null && Server.CurrentTime - menu.StartTime > _hPlugin.Config.MenuTime)
            {
                if (_hPlugin.Config.StopingUser &&
                    player.PawnIsAlive &&
                    player.Pawn.Value != null &&
                    player.Pawn.Value.MoveType == MoveType_t.MOVETYPE_NONE)
                {
                    player.Pawn.Value.MoveType = MoveType_t.MOVETYPE_WALK;
                    Schema.SetSchemaValue(player.Pawn.Value.Handle, "CBaseEntity", "m_nActualMoveType", 2);
                    Utilities.SetStateChanged(player.Pawn.Value, "CBaseEntity", "m_MoveType");
                }

                menu.Close(true);
                Menus.RemoveAt(i);
                i--;
                continue;
            }

            PlayerButtons buttons = player.Buttons;
            if (_hPlugin is { Config.StopingUser: true } && player.PawnIsAlive && player.Pawn.Value != null &&
                player.Pawn.Value.MoveType == MoveType_t.MOVETYPE_WALK)
            {
                player.Pawn.Value.MoveType = MoveType_t.MOVETYPE_NONE;
                Schema.SetSchemaValue(player.Pawn.Value.Handle, "CBaseEntity", "m_nActualMoveType", 0);
                Utilities.SetStateChanged(player.Pawn.Value, "CBaseEntity", "m_MoveType");
            }

            if (!menu.IsEqualButtons(buttons.ToString()))
            {
                if (_hPlugin != null && buttons.HasFlag(_hPlugin.Config.ButtonsConfig.UpButton))
                {
                    _ = menu.MoveUp();
                }
                else if (_hPlugin != null && buttons.HasFlag(_hPlugin.Config.ButtonsConfig.DownButton))
                {
                    _ = menu.MoveDown();
                }
                else if (_hPlugin != null && buttons.HasFlag(_hPlugin.Config.ButtonsConfig.LeftButton))
                {
                    _ = menu.MoveUp(GetPlugin()!.Config.MenuLinesCount);
                }
                else if (_hPlugin != null && buttons.HasFlag(_hPlugin.Config.ButtonsConfig.RightButton))
                {
                    _ = menu.MoveDown(GetPlugin()!.Config.MenuLinesCount);
                }
                else if (_hPlugin != null && buttons.HasFlag(_hPlugin.Config.ButtonsConfig.SelectButton))
                {
                    menu.OnSelect();
                }
                else if (_hPlugin != null && buttons.HasFlag(_hPlugin.Config.ButtonsConfig.BackButton))
                {
                    menu.Menu.BackAction?.Invoke(player);
                }

                if (_hPlugin != null && (buttons.HasFlag(_hPlugin.Config.ButtonsConfig.ExitButton) || menu.Closed()))
                {
                    menu.Close(true);
                    if (_hPlugin.Config.StopingUser && player.PawnIsAlive && player.Pawn.Value != null &&
                        player.Pawn.Value.MoveType == MoveType_t.MOVETYPE_NONE)
                    {
                        player.Pawn.Value.MoveType = MoveType_t.MOVETYPE_WALK;
                        Schema.SetSchemaValue(player.Pawn.Value.Handle, "CBaseEntity", "m_nActualMoveType", 2);
                        Utilities.SetStateChanged(player.Pawn.Value, "CBaseEntity", "m_MoveType");
                    }

                    Menus.RemoveAt(i);
                    i--;
                    continue;
                }
            }

            menu.GetPlayer().PrintToCenterHtml(menu.GetText(), 1);
        }
    }

    public static void PlaySound(CCSPlayerController player, string sound)
    {
        if (string.IsNullOrEmpty(sound))
        {
            return;
        }

        if (!Misc.GetPlayerSoundsEnabled(player))
        {
            return;
        }

        if (sound.StartsWith("sounds/"))
        {
            player.ExecuteClientCommand("play " + sound);
        }
        else
        {
            if (_hPlugin == null)
            {
                return;
            }

            _ = player.EmitSound(sound, player, _hPlugin.Config.SoundVolume);
        }
    }

    public static void CloseMenu(CCSPlayerController player)
    {
        if (player == null)
        {
            return;
        }

        CounterStrikeSharp.API.Modules.Menu.MenuManager.CloseActiveMenu(player);

        PlayerInfo? menuInfo = Menus.FirstOrDefault(t => t.GetPlayer() == player);
        if (menuInfo != null)
        {
            menuInfo.Close();

            if (_hPlugin is { Config.StopingUser: true } &&
                player.PawnIsAlive &&
                player.Pawn.Value != null &&
                player.Pawn.Value.MoveType == MoveType_t.MOVETYPE_NONE)
            {
                player.Pawn.Value.MoveType = MoveType_t.MOVETYPE_WALK;
                Schema.SetSchemaValue(player.Pawn.Value.Handle, "CBaseEntity", "m_nActualMoveType", 2);
                Utilities.SetStateChanged(player.Pawn.Value, "CBaseEntity", "m_MoveType");
            }
        }

        if (menuInfo != null)
        {
            _ = Menus.Remove(menuInfo);
        }

        MenusMm.ClosePlayerMenu(player.Slot);
    }

    internal static bool Freeze(CCSPlayerController player)
    {
        if (!player.PawnIsAlive || player.Pawn.Value == null || player.Pawn.Value.MoveType != MoveType_t.MOVETYPE_WALK)
        {
            return false;
        }

        player.Pawn.Value.MoveType = MoveType_t.MOVETYPE_NONE;
        Schema.SetSchemaValue(player.Pawn.Value.Handle, "CBaseEntity", "m_nActualMoveType", 0);
        Utilities.SetStateChanged(player.Pawn.Value, "CBaseEntity", "m_MoveType");
        return true;
    }

    internal static void Unfreeze(CCSPlayerController player)
    {
        if (!player.PawnIsAlive || player.Pawn.Value == null || player.Pawn.Value.MoveType != MoveType_t.MOVETYPE_NONE)
        {
            return;
        }

        player.Pawn.Value.MoveType = MoveType_t.MOVETYPE_WALK;
        Schema.SetSchemaValue(player.Pawn.Value.Handle, "CBaseEntity", "m_nActualMoveType", 2);
        Utilities.SetStateChanged(player.Pawn.Value, "CBaseEntity", "m_MoveType");
    }

    internal static bool HasOpenedMenu(CCSPlayerController player, PlayerInfo? info = null)
    {
        return Menus.Any(menu => menu.GetPlayer() == player && !menu.Closed() && menu != info) || (info == null && MenusMm.IsMenuOpen(player.Slot));
    }

    internal static void Init(MenuManagerCore hPlugin)
    {
        _hPlugin = hPlugin;
    }

    internal static MenuManagerCore? GetPlugin()
    {
        return _hPlugin;
    }
}
