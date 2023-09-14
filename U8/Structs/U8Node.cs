using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace U8.Structs
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct U8_node
    {
        public UInt16 type; //this is really a u8
        public UInt16 name_offset; //really a "u24"
        public UInt32 data_offset;
        public UInt32 size;

        public void ReverseEndianness()
        {
            type = BinaryPrimitives.ReverseEndianness(type);
            name_offset = BinaryPrimitives.ReverseEndianness(name_offset);
            data_offset = BinaryPrimitives.ReverseEndianness(data_offset);
            size = BinaryPrimitives.ReverseEndianness(size);
        }
    };
}
