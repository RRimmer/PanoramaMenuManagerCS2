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

    private readonly PhraseSlot _titleSlot = new();
    private readonly Dictionary<ChatMenuOption, PhraseSlot> _optionSlots = new();

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
        ResolvePhrases(player);
        IMenu? menu = null;

        // WeaponPaints keeps one menu object and opens it again. Resolve Default and
        // ButtonMenu on every open, otherwise the first player's type sticks forever.
        // IksAdmin MenuType 3 calls NewMenuForcetype(ButtonMenu). That value is the
        // old WASD/center-HTML menu. Follow the configured menu (Panorama) instead.
        MenuType type = _forcetype;
        if (type is MenuType.Default or MenuType.ButtonMenu)
        {
            type = Misc.GetCurrentPlayerMenu(player);
        }

        if (type == MenuType.MetamodMenu && !MenusMm.Hooked())
        {
            type = MenuType.ButtonMenu;
        }

        if (type == MenuType.CsgoMenu)
        {
            CsgoMenu.Show(player, this);
            return;
        }

        // Mouse panorama only. A module such as PMM_WeaponPaints may draw this
        // menu itself. The option callbacks stay the ones the other plugin set.
        if (type == MenuType.PanoramaMenu)
        {
            Control.CloseMenu(player);
            CsgoMenu.Close(player);
            PanoramaHud.CloseMenu(player);
            if (PaintRegistry.TryPaint(player, this))
            {
                return;
            }
        }

        menu = type switch
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

        if (type == MenuType.ButtonMenu)
        {
            ((ButtonMenu)menu).BackAction = OnBackAction;
            ((ButtonMenu)menu).ResetAction = OnResetAction;
        }
        else
        {
            bool flag = type == MenuType.CenterMenu;
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

        if ((type is MenuType.PanoramaMenu or MenuType.PanoramaWasdMenu) &&
            PanoramaHud.Show(player, this, type == MenuType.PanoramaWasdMenu))
        {
            return;
        }

        if (Control.GetPlugin()!.Config.UseMetamodMenu &&
            ((Control.GetPlugin()!.Config.UseMetamodMenuReplace && type == MenuType.ButtonMenu) ||
             type == MenuType.MetamodMenu))
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

    private void ResolvePhrases(CCSPlayerController player)
    {
        Title = ResolveSlot(player, Title, _titleSlot);
        foreach (ChatMenuOption option in MenuOptions)
        {
            if (!_optionSlots.TryGetValue(option, out PhraseSlot? slot))
            {
                slot = new PhraseSlot();
                _optionSlots[option] = slot;
            }

            option.Text = ResolveSlot(player, option.Text, slot);
        }
    }

    private static string ResolveSlot(CCSPlayerController player, string current, PhraseSlot slot)
    {
        // The first open keeps the raw label (SA_SLAP) as the source. While the
        // visible text is still the phrase from the last open, the next player
        // is translated again. A plugin that replaces the label becomes the new source.
        if (!string.Equals(current, slot.Applied, StringComparison.Ordinal))
        {
            slot.Source = current;
        }

        string applied = ModuleTranslations.Apply(player, slot.Source ?? current);
        slot.Applied = applied;
        return applied;
    }

    private sealed class PhraseSlot
    {
        public string? Source;
        public string? Applied;
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