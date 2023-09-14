using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using U8.Helpers;
using U8.Structs;
using InMemoryBinaryFile.Helpers;
using XBFLib;

namespace U8
{
    //TODO Rewrite
    public class U8Unpacker
    {
        protected T ReadStruct<T>(BinaryReader reader)
            where T : struct
        {
            return StructHelper.CastToStruct<T>(reader.ReadBytes(Marshal.SizeOf<T>()));
        }
        protected T[] ReadStructArray<T>(BinaryReader reader, uint length)
            where T : struct
        {
            T[] result = new T[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = ReadStruct<T>(reader);
            }
            return result;
        }

        protected bool IsNestedArc(byte[] content)
        {
            return content.Length > Marshal.SizeOf<U8ArchiveHeader>()
                &&
                content[0] == 0x55
                &&
                content[1] == 0xAA
                &&
                content[2] == 0x38
                &&
                content[3] == 0x2D;
        }

        private readonly BinaryReader reader;
        private readonly string outputDirectory;

        public U8Unpacker(BinaryReader reader, string outputDirectory)
        {
            this.reader = reader;
            this.outputDirectory = outputDirectory;

            Directory.CreateDirectory(outputDirectory);
        }

        private U8ArchiveHeader Header;
        private U8Node RootNode;
        private U8Node[] Nodes;
        private byte[] StringTable;
        private int totalParsedNodes;

        public void Unpack()
        {
            reader.BaseStream.Seek(0, SeekOrigin.Begin);

            Header = ReadStruct<U8ArchiveHeader>(reader);
            RootNode = ReadStruct<U8Node>(reader);

            Header.ReverseEndianness();
            RootNode.ReverseEndianness();

            Nodes = ReadStructArray<U8Node>(reader, RootNode.size - 1);

            var stringTableSize = Header.data_offset - Marshal.SizeOf<U8ArchiveHeader>() - RootNode.size * Marshal.SizeOf<U8Node>();
            StringTable = reader.ReadBytes((int)stringTableSize);

            var nodeEnumerator = Nodes.Cast<U8Node>().GetEnumerator();
            totalParsedNodes = 1; //rootNode already parsed
            while (totalParsedNodes < RootNode.size)
            {
                ParseNode(outputDirectory, nodeEnumerator);
            }
        }

        private void ParseNode(string directoryPath, IEnumerator<U8Node> nodeEnumerator)
        {
            nodeEnumerator.MoveNext();
            var node = nodeEnumerator.Current;

            // workaround for bug from .arc modified in BrawlCrate
            if (
                node.type == 0
                &&
                node.data_offset == 0
                &&
                node.size == 0
                &&
                node.name_offset == 0)
            {
                nodeEnumerator.MoveNext();
            }

            node.ReverseEndianness();

            var name = StringTable.FindNullTerminatedString(start: node.name_offset).ToShiftJisString();
            //var name = StringBytesHelper.UnsafeAsciiBytesToString(StringTable, node.name_offset);

            //256 = directory
            if (node.type == 0x0100)
            {
                var directory = Path.Combine(directoryPath, name);
                Directory.CreateDirectory(directory);
                totalParsedNodes++;
                while (totalParsedNodes < node.size)
                {
                    ParseNode(directory, nodeEnumerator);
                }
                //totalParsedNodes should be equal to node.size;
            }
            else
            {
                //0 = nested arc or maybe just any file?

                totalParsedNodes++;

                reader.BaseStream.Seek(node.data_offset, SeekOrigin.Begin);
                var fullPath = Path.Combine(directoryPath, name);
                var contentBytes = reader.ReadBytes((int)node.size);

                if (name.EndsWith(".arc") && IsNestedArc(contentBytes))
                {
                    using (var nestedStream = new MemoryStream(contentBytes))
                    using (var nestedReader = new BinaryReader(nestedStream))
                    {
                        var U8 = new U8Unpacker(nestedReader, fullPath);
                        U8.Unpack();
                    }
                }
                else
                {
                    using (var output = System.IO.File.OpenWrite(fullPath))
                    using (var writer = new BinaryWriter(output))
                    {
                        writer.Write(contentBytes);
                    }

                    if (fullPath.EndsWith(".xbf"))
                    {
                        var expected = File.ReadAllBytes(fullPath);

                        bool isUTF8 = fullPath.EndsWith("BlockText.xbf", StringComparison.OrdinalIgnoreCase);

                        var parsed = new XbfRootSegment(isUTF8);
                        parsed.Parse(expected.AsSpan());

                        var xmlFile = fullPath
                            .Replace(".xbf", ".xml");

                        if (isUTF8)
                        {
                            using (TextWriter sw = new StreamWriter(xmlFile, false, Encoding.UTF8)) //Set encoding
                            {
                                parsed.NodeTree.XmlDocument.Save(sw);
                            }
                        }
                        else
                        {
                            File.WriteAllText(xmlFile, parsed.NodeTree.ToString());
                        }
                    }
                }
            }
        }
    }
}
