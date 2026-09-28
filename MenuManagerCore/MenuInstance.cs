using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using MenuManager;

namespace MenuManagerCore;

public class MenuInstance(
    string title,
    Action<CCSPlayerController>? backAction = null,
    Action<CCSPlayerController>? resetAction = null,
    MenuType forcetype = MenuType.Default)
    : IMenu
{
    public readonly Action<CCSPlayerController>? BackAction = backAction;
    public readonly Action<CCSPlayerController>? ResetAction = resetAction;
    private MenuType _forcetype = forcetype;

    public string Title { get; set; } = title;

    public List<ChatMenuOption> MenuOptions { get; } = [];

    public bool ExitButton { get; set; } = true;

    public PostSelectAction PostSelectAction { get; set; } = PostSelectAction.Nothing;

    public ChatMenuOption AddMenuOption(string display, Action<CCSPlayerController, ChatMenuOption> onSelect,
        bool disabled = false)
    {
        ChatMenuOption option = new(display, disabled, onSelect);
        MenuOptions.Add(option);
        return option;
    }

    public void Open(CCSPlayerController player)
    {
        IMenu? menu = null;

        // IksAdmin MenuType 3 calls NewMenuForcetype(ButtonMenu). That value is the
        // old WASD/center-HTML menu. Follow the configured menu (Panorama) instead.
        if (_forcetype is MenuType.Default or MenuType.ButtonMenu)
        {
            _forcetype = Misc.GetCurrentPlayerMenu(player);
        }

        if (_forcetype == MenuType.MetamodMenu && !MenusMm.Hooked())
        {
            _forcetype = MenuType.ButtonMenu;
        }

        if (_forcetype == MenuType.CsgoMenu)
        {
            CsgoMenu.Show(player, this);
            return;
        }

        menu = _forcetype switch
        {
            MenuType.ChatMenu => new ChatMenu(Title),
            MenuType.ConsoleMenu => new ConsoleMenu(Title),
            MenuType.CenterMenu => new CenterHtmlMenu(Title,
                Control.GetPlugin() ?? throw new InvalidOperationException()),
            MenuType.ButtonMenu => new ButtonMenu(Title),
            MenuType.MetamodMenu => new ButtonMenu(Title, true),
            MenuType.PanoramaMenu => new ButtonMenu(Title),
            MenuType.PanoramaWasdMenu => new ButtonMenu(Title),
            _ => menu
        };

        if (menu == null)
        {
            return;
        }

        menu.ExitButton = ExitButton;
        menu.PostSelectAction = PostSelectAction;

        if (BackAction != null)
        {
            _ = menu.AddMenuOption(
                Control.GetPlugin()?.Localizer["menumanager.back"] ?? throw new InvalidOperationException(),
                (p, _) =>
                {
                    if (p != null)
                    {
                        OnBackAction(p);
                    }
                });
        }

        if (_forcetype == MenuType.ButtonMenu)
        {
            ((ButtonMenu)menu).BackAction = OnBackAction;
            ((ButtonMenu)menu).ResetAction = OnResetAction;
        }
        else
        {
            bool flag = _forcetype == MenuType.CenterMenu;
            menu.Title = Misc.ColorText(menu.Title, flag);
            foreach (ChatMenuOption t in MenuOptions)
            {
                t.Text = Misc.ColorText(t.Text, flag);
            }
        }

        foreach (ChatMenuOption option in MenuOptions)
        {
            _ = menu.AddMenuOption(option.Text, option.OnSelect, option.Disabled);
        }

        if ((_forcetype is MenuType.PanoramaMenu or MenuType.PanoramaWasdMenu) &&
            PanoramaHud.Show(player, this, _forcetype == MenuType.PanoramaWasdMenu))
        {
            return;
        }

        if (Control.GetPlugin()!.Config.UseMetamodMenu &&
            ((Control.GetPlugin()!.Config.UseMetamodMenuReplace && _forcetype == MenuType.ButtonMenu) ||
             _forcetype == MenuType.MetamodMenu))
        {
            MenusMm.PassMenuToMm(player, this);
        }
        else
        {
            MenusMm.ClosePlayerMenu(player.Slot);
            menu.Open(player);
        }
    }

    public void OpenToAll()
    {
        foreach (CCSPlayerController player in Misc.GetValidPlayers())
        {
            Open(player);
        }
    }

    private void OnBackAction(CCSPlayerController player)
    {
        string? configSoundBack = Control.GetPlugin()?.Config.SoundBack;
        if (configSoundBack != null)
        {
            Control.PlaySound(player, configSoundBack);
        }

        BackAction?.Invoke(player);
    }

    private void OnResetAction(CCSPlayerController player)
    {
        ResetAction?.Invoke(player);
    }
}