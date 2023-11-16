using InMemoryBinaryFile;
using InMemoryBinaryFile.Helpers;
using InMemoryBinaryFile.New;
using InMemoryBinaryFile.New.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace XBFLib.New
{
    [BinarySegment(HeaderOffset = 0, BodyOffset = 0, Length = 4)]
    public class TreeNode : BinarySegment<XbfFile>
    {
        public TreeNode(XbfFile parent) : base(parent)
        {
        }

        [BinaryFieldAttribute(Offset = 0, OffsetScope = OffsetScope.Segment)]
        public short NameId { get; set; }
        [BinaryFieldAttribute(Offset = 2, OffsetScope = OffsetScope.Segment)]
        public ushort ValueId { get; set; }

        public bool IsClosingTag => ValueId == 0xFFFF;
        public bool IsAttribute => NameId < 0;
    }

    [BinarySegment(HeaderOffset = 8, BodyOffset = 0x28)]
    public class XbfFile(Encoding encoding) : InMemoryBinaryFile.New.IBinarySegment
    {
        public XbfFile() : this(Encoding.UTF8)
        {
            
        }
        public string GetXmlString()
        {
            return System.Xml.Linq.XElement.Parse(GetXmlDocument().OuterXml).ToString();
        }
        public XmlDocument GetXmlDocument()
        {
            var doc = new XmlDocument();

            XmlNode node = doc;
            foreach (var entry in Tree)
            {
                if (entry.IsClosingTag)
                {
                    if (node.ParentNode == null)
                    {
                        throw new Exception($"Error in XML tree. Parent of {node.Name} is null.");
                    }
                    node = node.ParentNode;
                }
                else if (entry.IsAttribute)
                {
                    var attributeName = AttributeDict.ElementAt(entry.NameId * -1).Value;
                    var attributeValue = StringDict.ElementAt(entry.ValueId).Value;
                    XmlElement el = (XmlElement)node;
                    el.SetAttribute(attributeName, attributeValue);
                }
                else
                {
                    var n = doc.CreateNode(XmlNodeType.Element, NodeDict.ElementAt(entry.NameId).Value, null);
                    n.InnerText = StringDict.ElementAt(entry.ValueId).Value;
                    node.AppendChild(n);
                    node = n;
                }
            }

            return doc;
        }

        public override string ToString()
        {
            return GetXmlString();
        }

        [NullTerminatedString(ExpectedValue = "XBF", Offset = 4 * 0, Order = -1)]
        public string? MagicNullTerminated { get; set; }
        [FixedLengthString(ExpectedValue = "XBF", Offset = 4 * 0, Order = -1, Length = 3)]
        public string? MagicFixedLength { get; set; }
        [BinaryFieldAttribute(ExpectedValue = 0x58_42_46_00, Offset = 4 * 0, Order = -1)]
        public int Magic { get; set; }

        [BinaryFieldAttribute(ExpectedValue = 0x03_00_80_00, Offset = 4 * 1, Order = -1)]
        public int Magic2 { get; set; }

        [BinaryFieldAttribute(Offset = 4 * 2)]
        public int TreeOffset { get; set; }
        [BinaryFieldAttribute(Offset = 4 * 3)]
        public int TreeCount { get; set; }
        public int TreeLength => 4 * TreeCount;

        [BinaryFieldAttribute()]
        public List<TreeNode>? Tree { get; set; }

        [BinaryFieldAttribute(Offset = 4 * 4)]
        public int NodeDictOffset { get; set; }
        [BinaryFieldAttribute(Offset = 4 * 5)]
        public int NodeDictCount { get; set; }

        [BinaryFieldAttribute(Offset = 4 * 6)]
        public int AttributeDictOffset { get; set; }
        [BinaryFieldAttribute(Offset = 4 * 7)]
        public int AttributeDictCount { get; set; }

        [BinaryFieldAttribute(Offset = 4 * 8)]
        public int StringDictOffset { get; set; }
        [BinaryFieldAttribute(Offset = 4 * 9)]
        public int StringDictCount { get; set; }

        [NullTerminatedString(Order = 1, OffsetZone = OffsetZone.Absolute)]
        public Dictionary<int, string>? AttributeDict { get; set; }
        [NullTerminatedString(Order = 1, OffsetZone = OffsetZone.Absolute)]
        public Dictionary<int, string>? StringDict { get; set; }
        [NullTerminatedString(Order = 1, OffsetZone = OffsetZone.Absolute)]
        public Dictionary<int, string>? NodeDict { get; set; }

        public Encoding NodeDictEncoding { get; } = encoding;
        public Encoding StringDictEncoding { get; } = encoding;
        public Encoding AttributeDictEncoding { get; } = encoding;
    }
}
