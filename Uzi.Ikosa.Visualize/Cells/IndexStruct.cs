using System.Runtime.InteropServices;

namespace Uzi.Visualize
{
    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    public struct IndexStruct
    {
        [FieldOffset(0)]
        public uint Index;
        [FieldOffset(4)]
        public uint StateInfo;
        [FieldOffset(0)]
        public ulong ID;
    }
}
