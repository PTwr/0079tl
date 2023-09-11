using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace U8.Structs
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct U8ArchiveHeader
    {
        public UInt32 tag; // 0x55AA382D "U.8-"
        public UInt32 rootnode_offset; // offset to root_node, always 0x20.
        public UInt32 header_size; // size of header from root_node to end of string table.
        public UInt32 data_offset; // offset to data -- this is rootnode_offset + header_size, aligned to 0x40.

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1 * 16)]
        byte[] zeroes;

        public void ReverseEndianness()
        {
            tag = BinaryPrimitives.ReverseEndianness(tag);
            rootnode_offset = BinaryPrimitives.ReverseEndianness(rootnode_offset);
            header_size = BinaryPrimitives.ReverseEndianness(header_size);
            data_offset = BinaryPrimitives.ReverseEndianness(data_offset);
        }
    };
}
