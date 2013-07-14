using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Key2Screen
{
    /// <summary>
    /// A class that manages a global low level keyboard hook
    /// </summary>
    internal class KeyboardHook
    {
        #region Constants and Fields

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYDOWN = 0x104;
        private const int WM_SYSKEYUP = 0x105;

        /// <summary>
        /// The collections of keys to watch for
        /// </summary>
        private static readonly List<Keys> HookedKeys = new List<Keys>();

        private static readonly KeyboardHookProc _reference = HookProc;

        /// <summary>
        /// Handle to the hook, need this to unhook and call the next hook
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")]
        private static IntPtr hhook = IntPtr.Zero;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="globalKeyboardHook"/> class and installs the keyboard hook.
        /// </summary>
        public KeyboardHook()
        {
            PressedKeys = new ConcurrentQueue<KeyEventData>();
            Hook();
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="globalKeyboardHook"/> is reclaimed by garbage collection and uninstalls the keyboard hook.
        /// </summary>
        ~KeyboardHook()
        {
            Unhook();
        }

        #endregion

        #region Delegates

        /// <summary>
        /// defines the callback type for the hook
        /// </summary>
        public delegate int KeyboardHookProc(int code,
            int wParam,
            ref keyboardHookStruct lParam);

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether to hook all keys or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if all keys are hooked; otherwise, <c>false</c>.
        /// </value>
        public static bool HookAllKeys { get; set; }

        public static ConcurrentQueue<KeyEventData> PressedKeys { get; private set; }

        private static KeyEventData lastKeyEventQueued;
        private static bool windowsPressed;

        private static Keys modifier;
        private static bool windowsWasLastPressedKey;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The callback for the keyboard hook
        /// </summary>
        /// <param name="code">The hook code, if it isn't >= 0, the function shouldn't do anyting</param>
        /// <param name="wParam">The event type</param>
        /// <param name="lParam">The keyhook event information</param>
        /// <returns></returns>
        public static int HookProc(int code,
            int wParam,
            ref keyboardHookStruct lParam)
        {
            if (code >= 0)
            {
                var key = (Keys)lParam.vkCode;
                if (HookedKeys.Contains(key) || HookAllKeys)
                {
                    KeyEventData keyEventDataToEnqueue = null;

                    if ((wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN))
                    {
                        if (!HandleModifier(key, true))
                        {
                            keyEventDataToEnqueue = new KeyEventData(key, modifier, windowsPressed);
                        }
                    }
                    else if ((wParam == WM_KEYUP || wParam == WM_SYSKEYUP))
                    {
                        Debug.WriteLine(windowsWasLastPressedKey.ToString() + key);

                        if ((windowsWasLastPressedKey) && (Equals(key, Keys.LWin) || Equals(key, Keys.RWin)))
                        {
                            PressedKeys.Enqueue(new KeyEventData(key, modifier, windowsPressed));
                        }

                        HandleModifier(key, false);

                        // Workaround for Windows+L
                        if (Keys.L.Equals(key) && windowsPressed)
                        {
                            windowsPressed = false;
                        }
                    }
                    if (keyEventDataToEnqueue != null)
                    {
                        if (keyEventDataToEnqueue.Equals(lastKeyEventQueued)
                            && lastKeyEventQueued.Ignore)
                        {
                            lastKeyEventQueued.Ignore = !lastKeyEventQueued.Ignore;
                        }
                        else
                        {
                            lastKeyEventQueued = keyEventDataToEnqueue;
                            PressedKeys.Enqueue(keyEventDataToEnqueue);
                        }
                    }
                }
            }
            return CallNextHookEx(hhook, code, wParam, ref lParam);
        }

        /// <summary>
        /// Installs the global hook
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void Hook()
        {
            IntPtr hInstance = LoadLibrary("User32");
            hhook = SetWindowsHookEx(WH_KEYBOARD_LL, _reference, hInstance, 0);
        }

        private static bool HandleModifier(Keys key, bool pressed)
        {
            if (key == Keys.LShiftKey
                || key == Keys.RShiftKey)
            {
                if (pressed)
                {
                    modifier |= Keys.Shift;
                }
                else
                {
                    modifier &= ~Keys.Shift;
                }
                return true;
            }

            if (key == Keys.Alt
                || key == Keys.LMenu
                || key == Keys.RMenu)
            {
                if (pressed)
                {
                    modifier |= Keys.Alt;
                }
                else
                {
                    modifier &= ~Keys.Alt;
                }
                return true;
            }

            if (key == Keys.LControlKey
                || key == Keys.RControlKey)
            {
                if (pressed)
                {
                    modifier |= Keys.Control;
                }
                else
                {
                    modifier &= ~Keys.Control;
                }
                return true;
            }

            windowsWasLastPressedKey = false;

            if (key == Keys.LWin
                || key == Keys.RWin)
            {
                windowsPressed = pressed;
                windowsWasLastPressedKey = pressed;
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Uninstalls the global hook
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void Unhook()
        {
            UnhookWindowsHookEx(hhook);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Calls the next hook.
        /// </summary>
        /// <param name="idHook">The hook id</param>
        /// <param name="nCode">The hook code</param>
        /// <param name="wParam">The wparam.</param>
        /// <param name="lParam">The lparam.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "return"), DllImport("user32.dll")]
        private static extern int CallNextHookEx(IntPtr idHook,
            int nCode,
            int wParam,
            ref keyboardHookStruct lParam);

        /// <summary>
        /// Loads the library.
        /// </summary>
        /// <param name="lpFileName">Name of the library</param>
        /// <returns>A handle to the library</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "0"), DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string lpFileName);

        /// <summary>
        /// Sets the windows hook, do the desired event, one of hInstance or threadId must be non-null
        /// </summary>
        /// <param name="idHook">The id of the event you want to hook</param>
        /// <param name="callback">The callback.</param>
        /// <param name="hInstance">The handle you want to attach the event to, can be null</param>
        /// <param name="threadId">The thread you want to attach the event to, can be null</param>
        /// <returns>a handle to the desired hook</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            KeyboardHookProc callback,
            IntPtr hInstance,
            uint threadId);

        /// <summary>
        /// Unhooks the windows hook.
        /// </summary>
        /// <param name="hInstance">The hook handle that was returned from SetWindowsHookEx</param>
        /// <returns>True if successful, false otherwise</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1414:MarkBooleanPInvokeArgumentsWithMarshalAs"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        #endregion

        public struct keyboardHookStruct
        {
            #region Constants and Fields

            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;

            #endregion
        }
    }
}