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
            while (offset < Header.data_offset)
            {
                var strbytes = content.FindNullTerminator(offset);
                strs.Add(strbytes.ToAsciiString());
                offset += strbytes.Length + 1;
            }

            //var strsss = Nodes
            //    //.Where(i=>i.name_offset < stringTableSize)
            //    .Select(i => BinaryPrimitives.ReverseEndianness(i.name_offset))
            //    .Select(i => StringTableBytes.FindNullTerminatedString(i).ToAsciiString()).ToList();

            strs.Clear();
            for (int i = 0; i < Nodes.Length; i++)
            {
                //Nodes[i].ReverseEndianness();
                var nameoffset = BinaryPrimitives.ReverseEndianness(Nodes[i].name_offset);
                strs.Add(StringTableBytes.FindNullTerminator(nameoffset).ToAsciiString());
            }

            //var nodeEnumerator = Nodes.Cast<U8Node>().GetEnumerator();
            //totalParsedNodes = 1; //rootNode already parsed
            //while (totalParsedNodes < RootNode.size)
            //{
            //    ParseNode(outputDirectory, nodeEnumerator);
            //}
        }
    }
}
