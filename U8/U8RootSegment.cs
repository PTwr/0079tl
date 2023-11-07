using InMemoryBinaryFile;
using InMemoryBinaryFile.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using XBFLib;

namespace U8
{
    public class U8RootSegment : ParentBinarySegment<IBinarySegment, U8NodeSegment>
    {
        public static byte[] U8MagicNumber = new byte[] { 0x55, 0xAA, 0x38, 0x2D };
        const int headerLength = 3 * 4;
        public U8RootSegment(IBinarySegment? parent = null) : base(parent, U8MagicNumber, headerLength)
        {
        }

        public override void Parse(Span<byte> content, Span<byte> everything)
        {
            if (!content.StartsWithMagicNumber(MagicNumber))
            {
                throw new Exception($"Content does not start with '{MagicNumber}'");
            }
            ParseHeader(content.Slice(MagicNumber.Length, HeaderLength));
            ParseNodes(content);
        }

        public override IEnumerable<byte> GetBytes()
        {
            var headerbytes = Concatenate(
                U8MagicNumber,
                HeaderBytes).PadToAlignment(32);


            //var bodyBytesz = Concatenate(
            //    Nodes.Where(i => i.IsFile).Reverse().Skip(1).Reverse().SelectMany(i => i.BinaryData.PadToAlignment(32)),
            //    Nodes.Last().BinaryData
            //    ).ToArray();

            List<byte> bodyBytes = new List<byte>();

            int offsetCorrection = DataOffset;
            var fileNodes = Nodes.Where(i => i.IsFile).ToList();
            //for (int i = 0; i < fileNodes.Count; i++)
            //{
            //    int alignment = 32;

            //    if (i == fileNodes.Count - 1)
            //    {
            //        alignment = 1;
            //    }

            //    var b = fileNodes[i].BinaryData.PadToAlignment(alignment).ToArray();

            //    offsetCorrection -= fileNodes[i].BinaryData.Length;
            //    offsetCorrection += b.Length;

            //    fileNodes[i].DataOffset += offsetCorrection;

            //    bodyBytes.AddRange(b);
            //}

            int paddedByTotal = 0;
            for (int i = 0; i < fileNodes.Count; i++)
            {
                fileNodes[i].DataOffset = DataOffset + bodyBytes.Count;
                bodyBytes.AddRange(fileNodes[i].BinaryData);

                //last node needs no padding
                if (i == fileNodes.Count - 1)
                {
                    continue;
                }

                int pad = (32 - bodyBytes.Count % 32) % 32;
                bodyBytes.AddRange(Enumerable.Repeat<byte>(0, pad));
                paddedByTotal += pad;
            }

            var nodeListBytes = Concatenate(
                Nodes.SelectMany(i => i.GetBytes()),
                dict.GetBytes()).PadToAlignment(32);

            return Concatenate(headerbytes, nodeListBytes, bodyBytes);
        }

        public List<U8NodeSegment> Nodes = new List<U8NodeSegment>();
        protected override void ParseBody(Span<byte> body, Span<byte> everything)
        {
            throw new NotImplementedException("unused");
        }

        public void DumpToDisk(string rootDir, bool parseXbf = true)
        {
            Directory.CreateDirectory(rootDir);
            foreach (var node in Nodes)
            {
                var path = Path.Join(rootDir, node.Path);
                if (node.IsDirectory)
                {
                    Directory.CreateDirectory(path);
                }
                else if (node.IsArc)
                {
                    var u8 = new U8RootSegment();
                    u8.Parse(node.BinaryData);
                    u8.DumpToDisk(path);
                }
                else if (node.IsFile)
                {
                    File.WriteAllBytes(path, node.BinaryData);

                    if (node.IsXbf && parseXbf)
                    {
                        var parsed = new XbfRootSegment(XbfRootSegment.ShouldBeUTF8(path));
                        parsed.Parse(node.BinaryData.AsSpan());

                        var xmlFile = path + ".xml";
                        parsed.DumpToDisk(xmlFile);
                    }
                }
            }
        }
        StringDictSegment<U8RootSegment> dict;
        private void ParseNodes(Span<byte> content)
        {
            int nodeChunkStart = U8MagicNumber.Length + headerLength + 16;
            var nodeCount = content.GetBigEndianDWORD(nodeChunkStart + 8);

            var stringBytes = content.Slice(nodeChunkStart + nodeCount * 12, NodeListAndStringDictLength - (nodeCount * 12));
            dict = new StringDictSegment<U8RootSegment>(this, EncodingHelper.Shift_JIS);
            dict.Parse(stringBytes);

            List<string> pathSegments = new List<string>();
            int dirEndNode = int.MaxValue;
            //todo check if multiple root nodes are possible?
            for (int i = 0; i < nodeCount; i++)
            {
                var node = new U8NodeSegment(this);
                node.Parse(content.Slice(nodeChunkStart + 12 * i));
                node.Name = dict[node.NameOffset];

                if (node.Size > dirEndNode && node.IsDirectory)
                {
                    pathSegments.RemoveAt(pathSegments.Count - 1);
                }
                node.Path = Path.Join(Path.Join(pathSegments.ToArray()), node.Name);

                if (node.IsDirectory)
                {
                    dirEndNode = node.Size;
                    pathSegments.Add(node.Name);
                }
                if (node.IsFile)
                {
                    node.BinaryData = content
                        .Slice(node.DataOffset, node.Size)
                        .ToArray();
                }

                Nodes.Add(node);
            }
        }

        protected override void ParseHeader(Span<byte> header, Span<byte> everything)
        {
            RootNodeOffset = header.GetBigEndianDWORD(0);
            NodeListAndStringDictLength = header.GetBigEndianDWORD(4);
            DataOffset = header.GetBigEndianDWORD(8);

            if (RootNodeOffset != 0x20)
            {
                throw new Exception($"$Expected RootNodeOffset=0x20. Actual = {RootNodeOffset:X2}");
            }
        }

        protected override IEnumerable<byte> HeaderBytes =>
            Concatenate(
                RootNodeOffset.GetBigEndianBytes(),
                NodeListAndStringDictLength.GetBigEndianBytes(),
                DataOffset.GetBigEndianBytes()
                );

        // offset to root_node, always 0x20.
        public int RootNodeOffset { get; private set; }
        // size of header from root_node to end of string table.
        public int NodeListAndStringDictLength { get; private set; }
        // offset to data -- this is rootnode_offset + header_size, aligned to 32byte?.
        public int DataOffset { get; private set; }
    }
}
