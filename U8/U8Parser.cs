using InMemoryBinaryFile.Helpers;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using U8.Helpers;
using U8.Structs;

namespace U8
{
    public class U8Parser
    {
        public void Parse(Span<byte> content)
        {
            //TODO offset out parameters SUUUUCKS, maybe out sliced span? but that will require using span sliced only from start
            var Header = content.ReadStruct<U8ArchiveHeader>(out var o);
            var offset = o;
            var RootNode = content.Slice(offset).ReadStruct<U8Node>(out o);
            offset += o;

            //todo fuck marshaling and use binary segment lib instead
            Header.ReverseEndianness();
            RootNode.ReverseEndianness();

            var Nodes = content.Slice(offset).ReadStructArray<U8Node>(RootNode.size - 1, out o);
            offset += o;

            var stringTableSize = Header.data_offset - Marshal.SizeOf<U8ArchiveHeader>() - RootNode.size * Marshal.SizeOf<U8Node>();
            var StringTableBytes = content.Slice(offset, (int)stringTableSize);


            List<string> strs = new List<string>();

            for (int i = 0; i < Nodes.Length; i++)
            {
                var nameoffset = BinaryPrimitives.ReverseEndianness(Nodes[i].name_offset);
                strs.Add(StringTableBytes.FindNullTerminator(nameoffset).ToAsciiString());
            }
        }
    }
}
