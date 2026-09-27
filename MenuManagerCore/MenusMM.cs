using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace MenuManagerCore;

// typedef std::function<void(const char* szBack, const char* szFront, int iItem, int iSlot)> MenuCallbackFunc;
//private static unsafe delegate* unmanaged[Cdecl]<string, string, int, int, void> MM_MenuCallbackFunc;

internal static class MenusMm
{
    public delegate void MmMenuCallbackFunc(string szBack, string szFront, int iItem, int iSlot);


    private static List<CallbackInfo> _callbacksInfos = [];

    private static unsafe delegate* unmanaged[Cdecl]<int, bool> _nativeIsMenuOpen;
    private static unsafe delegate* unmanaged[Cdecl]<int, void> _nativeClosePlayerMenu;

    private static unsafe delegate* unmanaged[Cdecl]<string, string, int, void>
        _nativeAddItemMenu; // MenusApi_SetExitMenu(Menu& hMenu, bool bExit)

    private static unsafe delegate* unmanaged[Cdecl]<bool, void>
        _nativeSetExitMenu; // MenusApi_SetExitMenu(Menu& hMenu, bool bExit)

    private static unsafe delegate* unmanaged[Cdecl]<bool, void>
        _nativeSetBackMenu; // MenusApi_SetBackMenu(Menu& hMenu, bool bBack)

    private static unsafe delegate* unmanaged[Cdecl]<string, void>
        _nativeSetTitleMenu; // MenusApi_SetTitleMenu(Menu& hMenu, const char* szTitle)

    private static unsafe delegate* unmanaged[Cdecl]<MmMenuCallbackFunc, void>
        _nativeSetCallback; // MenusApi_SetCallback(Menu& hMenu, MenuCallbackFunc func)

    private static unsafe delegate* unmanaged[Cdecl]<int, bool, bool, void>
        _nativeDisplayPlayerMenu; // MenusApi_DisplayPlayerMenu(Menu& hMenu, int iSlot, bool bClose = true, bool bReset = true)

    private static unsafe delegate* unmanaged[Cdecl]<int, void>
        _nativeNewMenuInstance; // MenusApi_DisplayPlayerMenu(Menu& hMenu, int iSlot, bool bClose = true, bool bReset = true)

    private static unsafe delegate* unmanaged[Cdecl]<int, void>
        _nativeClear; // MenusApi_DisplayPlayerMenu(Menu& hMenu, int iSlot, bool bClose = true, bool bReset = true)

    private static bool _hooked = false;

    private static string GetOsExt()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "so" : "dll";
    }

    internal static void Init()
    {
        if (_hooked)
        {
            return;
        }

        if (_callbacksInfos == null)
        {
            _callbacksInfos = [];
        }
        else
        {
            _callbacksInfos.Clear();
        }

        string libPath = $"{Server.GameDirectory}/csgo/addons/MenusExport/bin/MenusExport.{GetOsExt()}";
        if (!File.Exists(libPath))
        {
            return;
        }

        nint libHandle = NativeLibrary.Load(libPath);
        if (libHandle != IntPtr.Zero)
        {
            nint funcPtr = NativeLibrary.GetExport(libHandle, "MenusApi_IsMenuOpen");
            if (funcPtr != IntPtr.Zero)
            {
                unsafe
                {
                    _nativeIsMenuOpen = (delegate* unmanaged[Cdecl]<int, bool>)funcPtr;
                }
            }
            else
            {
                NotHooked(2);
                return;
            }

            funcPtr = NativeLibrary.GetExport(libHandle, "MenusApi_ClosePlayerMenu");
            if (funcPtr != IntPtr.Zero)
            {
                unsafe
                {
                    _nativeClosePlayerMenu = (delegate* unmanaged[Cdecl]<int, void>)funcPtr;
                }
            }
            else
            {
                NotHooked(3);
                return;
            }


            funcPtr = NativeLibrary.GetExport(libHandle, "MenusApi_AddItemMenu");
            if (funcPtr != IntPtr.Zero)
            {
                unsafe
                {
                    _nativeAddItemMenu = (delegate* unmanaged[Cdecl]<string, string, int, void>)funcPtr;
                }
            }
            else
            {
                NotHooked(4);
                return;
            }

            funcPtr = NativeLibrary.GetExport(libHandle, "MenusApi_SetExitMenu");
            if (funcPtr != IntPtr.Zero)
            {
                unsafe
                {
                    _nativeSetExitMenu = (delegate* unmanaged[Cdecl]<bool, void>)funcPtr;
                }
            }
            else
            {
                NotHooked(4);
                return;
            }

            funcPtr = NativeLibrary.GetExport(libHandle, "MenusApi_SetBackMenu");
            if (funcPtr != IntPtr.Zero)
            {
                unsafe
                {
                    _nativeSetBackMenu = (delegate* unmanaged[Cdecl]<bool, void>)funcPtr;
                }
            }
            else
            {
                NotHooked(5);
                return;
            }

            funcPtr = NativeLibrary.GetExport(libHandle, "MenusApi_SetTitleMenu");
            if (funcPtr != IntPtr.Zero)
            {
                unsafe
                {
                    _nativeSetTitleMenu = (delegate* unmanaged[Cdecl]<string, void>)funcPtr;
                }
            }
            else
            {
                NotHooked(6);
                return;
            }

            funcPtr = NativeLibrary.GetExport(libHandle, "MenusApi_SetCallback");
            if (funcPtr != IntPtr.Zero)
            {
                unsafe
                {
                    _nativeSetCallback = (delegate* unmanaged[Cdecl]<MmMenuCallbackFunc, void>)funcPtr;
                }
            }
            else
            {
                NotHooked(7);
                return;
            }

            funcPtr = NativeLibrary.GetExport(libHandle, "MenusApi_DisplayPlayerMenu");
            if (funcPtr != IntPtr.Zero)
            {
                unsafe
                {
                    _nativeDisplayPlayerMenu = (delegate* unmanaged[Cdecl]<int, bool, bool, void>)funcPtr;
                }
            }
            else
            {
                NotHooked(8);
                return;
            }

            funcPtr = NativeLibrary.GetExport(libHandle, "MenusApi_NewMenuInstance");
            if (funcPtr != IntPtr.Zero)
            {
                unsafe
                {
                    _nativeNewMenuInstance = (delegate* unmanaged[Cdecl]<int, void>)funcPtr;
                }
            }
            else
            {
                NotHooked(9);
                return;
            }

            funcPtr = NativeLibrary.GetExport(libHandle, "MenusApi_Clear");
            if (funcPtr != IntPtr.Zero)
            {
                unsafe
                {
                    _nativeClear = (delegate* unmanaged[Cdecl]<int, void>)funcPtr;
                }
            }
            else
            {
                NotHooked(10);
                return;
            }

        }
        else
        {
            NotHooked(1);
            return;
        }

        Control.GetPlugin()?.Logger.LogInformation("====================================");
        Control.GetPlugin()?.Logger.LogInformation(" ");
        Control.GetPlugin()?.Logger.LogInformation("Metamod MenusApi found and hooked!");
        Control.GetPlugin()?.Logger.LogInformation(" ");
        Control.GetPlugin()?.Logger.LogInformation("====================================");

        NativeLibrary.Free(libHandle);
        _hooked = true;
    }

    internal static bool IsMenuOpen(int iSlot)
    {
        if (_hooked)
        {
            unsafe
            {
                return _nativeIsMenuOpen(iSlot);
            }
        }

        return false;
    }

    internal static void ClosePlayerMenu(int iSlot)
    {
        if (_hooked)
        {
            unsafe
            {
                if (_nativeIsMenuOpen(iSlot))
                {
                    _nativeClosePlayerMenu(iSlot);
                }
            }
        }
    }

    internal static void SetExitMenu(bool exit)
    {
        if (_hooked)
        {
            unsafe
            {
                _nativeSetExitMenu(exit);
            }
        }
    }

    internal static void SetBackMenu(bool back)
    {
        if (_hooked)
        {
            unsafe
            {
                _nativeSetBackMenu(back);
            }
        }
    }

    internal static void SetTitleMenu(string title)
    {
        if (_hooked)
        {
            unsafe
            {
                _nativeSetTitleMenu(title);
            }
        }
    }

    internal static void SetCallback(MmMenuCallbackFunc func)
    {
        if (_hooked)
        {
            unsafe
            {
                _nativeSetCallback(func);
            }
        }
    }

    internal static void DisplayPlayerMenu(int slot, bool close = true, bool reset = true)
    {
        if (_hooked)
        {
            unsafe
            {
                _nativeDisplayPlayerMenu(slot, close, reset);
            }
        }
    }

    internal static void AddItemMenu(string back, string text, bool disabled = false)
    {
        if (_hooked)
        {
            int itemtype = 1;
            if (disabled)
            {
                itemtype = 2;
            }

            unsafe
            {
                _nativeAddItemMenu(back, text, itemtype);
            }
        }
    }

    internal static void NewMenuInstance(int slot)
    {
        if (_hooked)
        {
            unsafe
            {
                _nativeNewMenuInstance(slot);
            }
        }
    }

    internal static void Clear(int slot)
    {
        if (_hooked)
        {
            unsafe
            {
                _nativeClear(slot);
            }
        }
    }

    internal static void NotHooked(int i)
    {
        _hooked = false;
        MenuManagerCore? plugin = Control.GetPlugin();
        if (plugin != null)
        {
            plugin.Logger.LogInformation("Metamod MenusApi found but couldnt hook it! [Code: {I}]", i);
            plugin.Config.UseMetamodMenu = false;
            plugin.Config.UseMetamodMenuReplace = false;
        }
    }

    private static void AddCallbackInfo(int slot, MmMenuCallbackFunc func)
    {
        _callbacksInfos.Add(new CallbackInfo(slot, func));
    }

    internal static void ClearCallbackInfo(int slot)
    {
        if (!_hooked)
        {
            return;
        }

        for (int i = _callbacksInfos.Count - 1; i >= 0; i--)
        {
            if (_callbacksInfos[i].Slot() == slot)
            {
                _callbacksInfos.RemoveAt(i);
            }
        }

        Clear(slot);
    }

    internal static bool Hooked()
    {
        return _hooked;
    }

    internal static void PassMenuToMm(CCSPlayerController player, MenuInstance menu)
    {
        if (!_hooked)
        {
            return;
        }

        int slot = player.Slot;
        NewMenuInstance(slot);
        SetTitleMenu(Misc.ColorText(menu.Title, false));
        if (menu.BackAction != null)
        {
            SetBackMenu(true);
        }

        SetExitMenu(menu.ExitButton);

        for (int i = 0; i < menu.MenuOptions.Count; i++)
        {
            AddItemMenu(i.ToString(), Misc.ColorText(menu.MenuOptions[i].Text, false), menu.MenuOptions[i].Disabled);
        }

        //Func<string, string, int, int, void> callback = delegate (string szBack, string szFront, int iItem, int iSlot)
        void callback(string szBack, string _, int iItem, int iSlot)
        {
            CCSPlayerController? player = Utilities.GetPlayerFromSlot(iSlot);

            if (player == null)
            {
                return;
            }

            if (iItem < 7)
            {
                int index = int.Parse(szBack);
                if (menu.PostSelectAction != PostSelectAction.Nothing)
                {
                    ClosePlayerMenu(iSlot);
                }

                menu.MenuOptions[index].OnSelect(player, menu.MenuOptions[index]);


                switch (menu.PostSelectAction)
                {
                    case PostSelectAction.Close:
                        ClosePlayerMenu(iSlot);
                        Control.CloseMenu(player);
                        break;
                    case PostSelectAction.Reset:
                        if (menu.ResetAction != null && !Control.HasOpenedMenu(player))
                        {
                            Server.NextFrame(() => menu.ResetAction(player));
                        }

                        break;
                }
            }
            else if (iItem == 7 && menu.BackAction != null)
            {
                menu.BackAction(player);
            }
        }

        AddCallbackInfo(slot, callback);
        SetCallback(_callbacksInfos.Last().Func);
        DisplayPlayerMenu(slot);
    }

    internal readonly struct CallbackInfo
    {
        private readonly int _slot;
        public readonly MmMenuCallbackFunc Func;

        internal CallbackInfo(int slot, MmMenuCallbackFunc func)
        {
            _slot = slot;
            Func = func;
        }

        internal int Slot()
        {
            return _slot;
        }
    }
}