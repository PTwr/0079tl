using InMemoryBinaryFile;
using InMemoryBinaryFile.Helpers;
using InMemoryBinaryFile.New;
using InMemoryBinaryFile.New.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XBFLib.New
{

    [BinarySegment(HeaderOffset = 0, BodyOffset = 0)]
    public class TreeNode(XbfFile parent) : IBinarySegment<XbfFile>
    {
        [BinaryFieldAttribute(Position = 0, SegmentOffset = SegmentOffset.Segment)]
        public short NameId { get; set; }
        [BinaryFieldAttribute(Position = 2, SegmentOffset = SegmentOffset.Segment)]
        public ushort ValueId { get; set; }

        public bool IsClosingTag => ValueId == 0xFFFF;
        public bool IsAttribute => NameId < 0;

        public XbfFile Parent { get; } = parent;
    }

    [BinarySegment(HeaderOffset = 8, BodyOffset = 0x28)]
    public class XbfFile(Encoding encoding) : IBinaryFile
    {
        [NullTerminatedString(ExpectedValue = "XBF", Position = 4 * 0, Order = -1)]
        public string? MagicNullTerminated { get; set; }
        [FixedLengthString(ExpectedValue = "XBF", Position = 4 * 0, Order = -1, Length = 3)]
        public string? MagicFixedLength { get; set; }

        [BinaryFieldAttribute(ExpectedValue = 0x58_42_46_00, Position = 4 * 0, Order = -1)]
        public int Magic { get; set; }

        [BinaryFieldAttribute(ExpectedValue = 0x03_00_80_00, Position = 4 * 1, Order = -1)]
        public int Magic2 { get; set; }

        [BinaryFieldAttribute(Position = 4 * 2)]
        public int TreePosition { get; set; }
        [BinaryFieldAttribute(Position = 4 * 3)]
        public int TreeCount { get; set; }

        [BinaryFieldAttribute()]
        public TreeNode[] Tree { get; set; }

        [BinaryFieldAttribute(Position = 4 * 4)]
        public int NodeDictPosition { get; set; }
        [BinaryFieldAttribute(Position = 4 * 5)]
        public int NodeDictCount { get; set; }

        [BinaryFieldAttribute(Position = 4 * 6)]
        public int AttributeDictPosition { get; set; }
        [BinaryFieldAttribute(Position = 4 * 7)]
        public int AttributeDictCount { get; set; }

        [BinaryFieldAttribute(Position = 4 * 8)]
        public int StringDictPosition { get; set; }
        [BinaryFieldAttribute(Position = 4 * 9)]
        public int StringDictCount { get; set; }

        [NullTerminatedString(Order = 1, FieldOffset = FieldOffset.Absolute)]
        public Dictionary<int, string>? AttributeDict { get; set; }
        [NullTerminatedString(Order = 1, FieldOffset = FieldOffset.Absolute)]
        public Dictionary<int, string>? StringDict { get; set; }
        [NullTerminatedString(Order = 1, FieldOffset = FieldOffset.Absolute)]
        public Dictionary<int, string>? NodeDict { get; set; }

        public Encoding NodeDictEncoding { get; } = encoding;
        public Encoding StringDictEncoding { get; } = encoding;
        public Encoding AttributeDictEncoding { get; } = encoding;
    }
}
