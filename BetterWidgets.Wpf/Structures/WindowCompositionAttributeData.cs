using BetterWidgets.Enums;
using System.Runtime.InteropServices;

namespace BetterWidgets.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }
}
