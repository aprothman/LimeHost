using System;
using System.Runtime.InteropServices;

namespace LimeWrapper
{
    internal static class GlWrapper
    {
        public const int PFD_DRAW_TO_WINDOW = 0x00000004;
        public const int PFD_SUPPORT_OPENGL = 0x00000020;
        public const int PFD_DOUBLEBUFFER   = 0x00000001;

        public const int PFD_MAIN_PLANE = 0x00000000;

        public const int PFD_TYPE_RGBA = 0x00000000;

        [StructLayout(LayoutKind.Sequential)]
        public struct PIXELFORMATDESCRIPTOR
        {
            public Int16 nSize;
            public Int16 nVersion;
            public Int32 dwFlags;
            public Byte iPixelType;
            public Byte cColorBits;
            public Byte cRedBits;
            public Byte cRedShift;
            public Byte cGreenBits;
            public Byte cGreenShift;
            public Byte cBlueBits;
            public Byte cBlueShift;
            public Byte cAlphaBits;
            public Byte cAlphaShift;
            public Byte cAccumBits;
            public Byte cAccumRedBits;
            public Byte cAccumGreenBits;
            public Byte cAccumBlueBits;
            public Byte cAccumAlphaBits;
            public Byte cDepthBits;
            public Byte cStencilBits;
            public Byte cAuxBuffers;
            public Byte iLayerType;
            public Byte bReserved;
            public Int32 dwLayerMask;
            public Int32 dwVisibleMask;
            public Int32 dwDamageMask;
        }

        [DllImport("User32.dll", EntryPoint = "GetDC", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("User32.dll", EntryPoint = "ReleaseDC", CallingConvention = CallingConvention.StdCall)]
        private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);
        [DllImport("Gdi32.dll", EntryPoint = "GetPixelFormat", CallingConvention = CallingConvention.StdCall)]
        private static extern int GetPixelFormat(IntPtr hdc);
        [DllImport("Gdi32.dll", EntryPoint = "DescribePixelFormat", CallingConvention = CallingConvention.StdCall)]
        private static extern int DescribePixelFormat(IntPtr hdc, int iPixelFormat, uint nBytes, IntPtr ppfd);
        [DllImport("Gdi32.dll", EntryPoint = "ChoosePixelFormat", CallingConvention = CallingConvention.StdCall)]
        private static extern int ChoosePixelFormat(IntPtr hdc, IntPtr ppfd);
        [DllImport("Gdi32.dll", EntryPoint = "SetPixelFormat", CallingConvention = CallingConvention.StdCall)]
        private static extern bool SetPixelFormat(IntPtr hdc, int format, IntPtr ppfd);

        public static bool SetupPixelFormat(IntPtr hwnd)
        {
            var dc = IntPtr.Zero;
            var hformat = default(GCHandle);

            try {
                dc = GetDC(hwnd);

                var format = default(PIXELFORMATDESCRIPTOR);
                format.nSize = (Int16)Marshal.SizeOf<PIXELFORMATDESCRIPTOR>();
                format.nVersion = 1;
                format.dwFlags = PFD_DRAW_TO_WINDOW
                               | PFD_SUPPORT_OPENGL
                               | PFD_DOUBLEBUFFER;
                format.iLayerType = PFD_MAIN_PLANE;
                format.iPixelType = PFD_TYPE_RGBA;
                format.cColorBits = 24;
                format.cStencilBits = 32;

                hformat = GCHandle.Alloc(format, GCHandleType.Pinned);
                var ppfd = hformat.AddrOfPinnedObject();
                
                var formatIdx = ChoosePixelFormat(dc, ppfd);

                return SetPixelFormat(dc, formatIdx, ppfd);
            }
            finally {
                if (hformat.IsAllocated) {
                    hformat.Free();
                }
                if (IntPtr.Zero != dc) {
                    ReleaseDC(hwnd, dc);
                }
            }
        }
    }
}
