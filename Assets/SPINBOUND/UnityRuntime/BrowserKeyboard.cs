using System.Runtime.InteropServices;

namespace Spinbound.UnityRuntime
{
    internal static class BrowserKeyboard
    {
        internal const int W = 1 << 0;
        internal const int A = 1 << 1;
        internal const int S = 1 << 2;
        internal const int D = 1 << 3;
        internal const int Up = 1 << 4;
        internal const int Left = 1 << 5;
        internal const int Down = 1 << 6;
        internal const int Right = 1 << 7;
        internal const int Shift = 1 << 8;
        internal const int Space = 1 << 9;
        internal const int Restart = 1 << 10;
        internal const int Escape = 1 << 11;

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern int Spinbound_GetKeyboardDownMask();

        [DllImport("__Internal")]
        private static extern int Spinbound_ConsumeKeyboardPressedMask();

        internal static int DownMask => Spinbound_GetKeyboardDownMask();
        internal static int ConsumePressedMask() => Spinbound_ConsumeKeyboardPressedMask();
#else
        internal static int DownMask => 0;
        internal static int ConsumePressedMask() => 0;
#endif
    }
}
