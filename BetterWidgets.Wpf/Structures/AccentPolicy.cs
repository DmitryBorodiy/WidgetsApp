using BetterWidgets.Enums;
using System.Runtime.InteropServices;

namespace BetterWidgets.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct AccentPolicy
    {
        public AccentState AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }
}
