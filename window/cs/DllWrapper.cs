using System;
using System.Runtime.InteropServices;

namespace LimeWrapper
{
    internal static class DllWrapper
    {
        private static bool _is64BitProcess = IntPtr.Size == 8;

        public static bool Is64Bit => _is64BitProcess;

        [DllImport("LimeEngine.dll", EntryPoint = "get_window_class", CallingConvention = CallingConvention.StdCall)]
        private static extern bool _getWindowClass(IntPtr wsClassName, Int32 size);
        private const int MAX_CLASS_LENGTH = 20;

        public static string GetWindowClass()
        {
            IntPtr wsName = IntPtr.Zero;

            try {
                wsName = Marshal.AllocHGlobal(MAX_CLASS_LENGTH * 2);

                if (_getWindowClass(wsName, MAX_CLASS_LENGTH)) {
                    return Marshal.PtrToStringUni(wsName);
                }

                return null;
            }
            finally {
                if (IntPtr.Zero != wsName) {
                    Marshal.FreeHGlobal(wsName);
                }
            }
        }

        [DllImport("LimeEngine.dll", EntryPoint = "init_haxe", CallingConvention = CallingConvention.StdCall)]
        public static extern int InitHaxe();
        [DllImport("LimeEngine.dll", EntryPoint = "init_window", CallingConvention = CallingConvention.StdCall)]
        public static extern int InitWindow();
        [DllImport("LimeEngine.dll", EntryPoint = "init_local_window", CallingConvention = CallingConvention.StdCall)]
        public static extern int InitLocalWindow(IntPtr hWnd);

        [DllImport("LimeEngine.dll", EntryPoint = "start_loop", CallingConvention = CallingConvention.StdCall)]
        public static extern void StartLoop();
        [DllImport("LimeEngine.dll", EntryPoint = "update", CallingConvention = CallingConvention.StdCall)]
        public static extern bool Update(int numEvents);

        public delegate void ExternalComListener(IntPtr callString);
        [DllImport("LimeEngine.dll", EntryPoint = "set_external_com_listener", CallingConvention = CallingConvention.StdCall)]
        public static extern void SetExternalComListener(ExternalComListener listener);
        [DllImport("LimeEngine.dll", EntryPoint = "call_external", CallingConvention = CallingConvention.StdCall)]
        public static extern void CallExternal(string call);

        [DllImport("LimeEngine.dll", EntryPoint = "exit_haxe", CallingConvention = CallingConvention.StdCall)]
        public static extern void ExitHaxe(int result);
    }
}
