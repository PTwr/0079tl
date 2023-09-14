using InMemoryBinaryFile.Helpers;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using U8.Helpers;
using U8.Structs;
using XBFLib;

namespace U8
{
    //TODO Rewrite
    //Updating Title_text ACE.arc node breaks later nodes....
    public class U8Updater
    {
        private readonly string outputDirectory;
        private readonly BinaryReader originalFile;
        private readonly BinaryWriter newFile;
        private readonly string initialIndentation;
        private U8Node RootNode;
        private U8Node[] Nodes;
        private int totalParsedNodes;
        private byte[] StringTable;

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

        public U8Updater(string outputDirectory, BinaryReader originalFile, BinaryWriter newFile, string initialIndentation = "")
        {
            this.outputDirectory = outputDirectory;
            this.originalFile = originalFile;
            this.newFile = newFile;
            this.initialIndentation = initialIndentation;
        }

        public void Update()
        {
            originalFile.BaseStream.Seek(0, SeekOrigin.Begin);
            newFile.BaseStream.Seek(0, SeekOrigin.Begin);

            var headerBytes = originalFile.ReadBytes(Marshal.SizeOf<U8ArchiveHeader>());
            var rootNodeBytes = originalFile.ReadBytes(Marshal.SizeOf<U8Node>());

            newFile.Write(headerBytes); //header
            newFile.Write(rootNodeBytes);

            var Header = StructHelper.CastToStruct<U8ArchiveHeader>(headerBytes);
            RootNode = StructHelper.CastToStruct<U8Node>(rootNodeBytes);

            Header.ReverseEndianness();
            RootNode.ReverseEndianness();

            var nodeSegmentStart = newFile.BaseStream.Position;
            var nodeBytes = originalFile.ReadBytes(Marshal.SizeOf<U8Node>() * ((int)RootNode.size - 1));
            newFile.Write(nodeBytes);
            originalFile.BaseStream.Seek(nodeSegmentStart, SeekOrigin.Begin);
            Nodes = ReadStructArray<U8Node>(originalFile, RootNode.size - 1);

            var stringTableSize = Header.data_offset - Marshal.SizeOf<U8ArchiveHeader>() - RootNode.size * Marshal.SizeOf<U8Node>();
            StringTable = originalFile.ReadBytes((int)stringTableSize);
            newFile.Write(StringTable);
            //return;

            var strs = Nodes
                //.Where(i=>i.name_offset < stringTableSize)
                .Select(i => BinaryPrimitives.ReverseEndianness(i.name_offset))
                .Select(i => StringTable.FindNullTerminatedString(i).ToAsciiString()).ToList();

            var nodeEnumerator = Nodes.Cast<U8Node>().GetEnumerator();
            totalParsedNodes = 1; //rootNode already parsed

            for (int i = 0; i < Nodes.Length; i++)
            {
                Nodes[i].ReverseEndianness();
            }

            for (nodeId = 0; nodeId < RootNode.size - 1; nodeId++)
            {
                ParseNode(outputDirectory, ref Nodes[nodeId], initialIndentation);
            }


            //return;
            //overwrite node segment after content length got updated
            var currentPos = newFile.BaseStream.Position;
            newFile.Seek((int)nodeSegmentStart, SeekOrigin.Begin);
            foreach (var node in Nodes)
            {
                node.ReverseEndianness();
                var bytes = StructHelper.CastToArray(node);
                var originalBytes = nodeBytes.Take(Marshal.SizeOf<U8Node>()).ToArray();
                newFile.Write(bytes);
            }
            newFile.Seek((int)currentPos, SeekOrigin.Begin);
        }
        int nodeId = 0;

        int dataOffsetDelta = 0;
        private void ParseNode(string directoryPath, ref U8Node node, string indentation = "")
        {

            //nodeEnumerator.MoveNext();
            //var node = nodeEnumerator.Current;

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
                return;
                //nodeEnumerator.MoveNext();
            }

            //node.ReverseEndianness();

            var name = StringTable.FindNullTerminatedString(start: node.name_offset).ToAsciiString();
            //var name = StringBytesHelper.UnsafeAsciiBytesToString(StringTable, node.name_offset);
            //Console.WriteLine($"Parsing node: {name}");

            var childName = StringTable.FindNullTerminatedString(start: Nodes[nodeId].name_offset).ToAsciiString();
            if (childName.EndsWith(".arc"))
            {
                Console.WriteLine($"{indentation}Parsing nested arc node #{nodeId}: {name}");
            }
            else
            {
                if (node.type == 0x0100)
                {
                    Console.WriteLine($"{indentation}Parsing directory #{nodeId}: {name}");
                }
                else
                {
                    Console.WriteLine($"{indentation}Parsing child node #{nodeId}: {name}");
                }
            }

            //256 = directory
            if (node.type == 0x0100)
            {
                nodeId++;

                var nodeSize = node.size;
                var names = string.Join(Environment.NewLine, Nodes
                    .Where((x, n) => n >= nodeId && n < nodeSize - 1)
                    .Select(i => StringTable.FindNullTerminatedString(start: i.name_offset).ToAsciiString()));

                //for (int i = nodeId; i < node.size - 1; i++)
                //{
                //    Console.WriteLine($"{indentation}Child node: {StringBytesHelper.UnsafeAsciiBytesToString(StringTable, Nodes[i].name_offset)}");
                //}

                var directory = Path.Combine(directoryPath, name);
                for (; nodeId < node.size - 1; nodeId++)
                {
                    ParseNode(directory, ref Nodes[nodeId], indentation + "  ");
                }
                //totalParsedNodes should be equal to node.size;

                nodeId--;
            }
            else
            {
                //Console.WriteLine($"Parsing node: {name}");
                //0 = nested arc or maybe just any file?

                if ((node.data_offset - dataOffsetDelta) > originalFile.BaseStream.Position)
                {
                    var somedatalength = node.data_offset - originalFile.BaseStream.Position;
                    var somedata = originalFile.ReadBytes((int)somedatalength);
                    newFile.Write(somedata);
                }

                originalFile.BaseStream.Seek(node.data_offset, SeekOrigin.Begin);
                var fullPath = Path.Combine(directoryPath, name);
                var contentBytes = originalFile.ReadBytes((int)node.size);

                if (name.EndsWith(".arc") && IsNestedArc(contentBytes))
                {
                    using (var nestedStream = new MemoryStream(contentBytes))
                    using (var nestedReader = new BinaryReader(nestedStream))
                    using (var nestedNewFileStream = new MemoryStream())
                    using (var nestedNewFileWriter = new BinaryWriter(nestedNewFileStream))
                    {
                        var U8 = new U8Updater(fullPath, nestedReader, nestedNewFileWriter, indentation + " ");
                        U8.Update();

                        var newBytes = nestedNewFileStream.ToArray();
                        contentBytes = newBytes;
                    }
                }
                else
                {
                    contentBytes = File.ReadAllBytes(fullPath);

                    var enXml = fullPath.Replace(".xbf", ".en.xml");
                    if (fullPath.EndsWith(".xbf") && File.Exists(enXml))
                    {
                        bool isUTF8 = enXml.EndsWith("BlockText.en.xml");

                        var xml = File.ReadAllText(enXml);
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(xml);

                        var parsed = new XbfRootSegment(doc, isUTF8);

                        contentBytes = parsed.GetBytes().ToArray();
                    }
                }

                node.data_offset = (uint)(node.data_offset + dataOffsetDelta);

                dataOffsetDelta += (int)(contentBytes.Length - node.size);
                node.size = (uint)contentBytes.Length;


                newFile.Write(contentBytes);
            }

            //eww struct are passed by value...
            //Nodes[totalParsedNodes - 2].ReverseEndianness();

            ////if previous nodes were shorter/longer we have to tweak offset
            //Nodes[totalParsedNodes - 2].data_offset = (uint)(Nodes[totalParsedNodes - 2].data_offset + dataOffsetDelta);
            ////Update data_offset tweak
            //dataOffsetDelta += (int)node.size - (int)Nodes[totalParsedNodes - 2].size; //uint math wants to return uint but we need negatives

            //Nodes[totalParsedNodes - 2].size = node.size;
            //Nodes[totalParsedNodes - 2].ReverseEndianness();
        }

    }
}
