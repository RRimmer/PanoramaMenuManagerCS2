using System.Runtime.CompilerServices;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;

namespace MenuManagerCore;

internal static class MenuOptionKind
{
    public const char ButtonMark = '\u200B';

    private static readonly string[] OnWords = ["on", "вкл", "açık", "acik"];
    private static readonly string[] OffWords = ["off", "выкл", "kapalı", "kapali"];
    private static readonly ConditionalWeakTable<ChatMenuOption, ToggleState> Toggles = new();
    private static readonly ConditionalWeakTable<ChatMenuOption, SelectState> Selects = new();

    private sealed class ToggleState
    {
        public bool On;
    }

    internal sealed class SelectState
    {
        public string Value = "";
        public string[] Choices = [];
        public Action<CCSPlayerController, ChatMenuOption, int>? OnSelect;
    }

    public static void SetToggle(ChatMenuOption option, bool on)
    {
        if (Toggles.TryGetValue(option, out ToggleState? state))
        {
            state.On = on;
            return;
        }

        Toggles.Add(option, new ToggleState { On = on });
    }

    public static bool TryGetToggle(ChatMenuOption option, out bool on)
    {
        if (Toggles.TryGetValue(option, out ToggleState? state))
        {
            on = state.On;
            return true;
        }

        on = false;
        return false;
    }

    public static void SetSelect(ChatMenuOption option, string value, string[] choices,
        Action<CCSPlayerController, ChatMenuOption, int> onSelect)
    {
        SelectState state = new()
        {
            Value = value,
            Choices = choices,
            OnSelect = onSelect
        };

        if (Selects.TryGetValue(option, out _))
        {
            Selects.Remove(option);
        }

        Selects.Add(option, state);
    }

    public static bool TryGetSelect(ChatMenuOption option, out SelectState state)
    {
        return Selects.TryGetValue(option, out state!);
    }

    public static bool TryParseToggleLabel(string text, out string label, out bool on)
    {
        label = text;
        on = false;
        if (text.Contains(ButtonMark))
        {
            return false;
        }

        int colon = text.LastIndexOf(':');
        if (colon <= 0 || colon >= text.Length - 1)
        {
            return false;
        }

        string name = text[..colon].Trim();
        string value = text[(colon + 1)..].Trim().TrimEnd('*').Trim();
        if (name.Length == 0 || value.Length == 0)
        {
            return false;
        }

        if (IsWord(value, OnWords))
        {
            label = name;
            on = true;
            return true;
        }

        if (IsWord(value, OffWords))
        {
            label = name;
            on = false;
            return true;
        }

        return false;
    }

    private static bool IsWord(string value, string[] words)
    {
        foreach (string word in words)
        {
            if (value.Equals(word, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
