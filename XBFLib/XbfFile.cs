using BinarySerializer;
using BinarySerializer.Annotation;
using BinarySerializer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace XBFLib
{
    [BinarySegment(HeaderOffset = 8, BodyOffset = 0x28)]
    public class XbfFile : IBinarySegment
    {
        public const string MagicNumber = "XBF";

        public XbfFile() : this(Encoding.UTF8)
        {

        }

        public XbfFile(Encoding encoding)
        {
            NodeDictEncoding = AttributeDictEncoding = StringDictEncoding = encoding;
        }

        public XbfFile(string xml, Encoding? encoding = null) : this(encoding ?? Encoding.UTF8)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            XmlToNodeList(doc.FirstChild);

            UpdateHeader();

        }
        public XbfFile(XmlDocument doc, Encoding? encoding = null) : this(encoding ?? Encoding.UTF8)
        {
            XmlToNodeList(doc.FirstChild);

            UpdateHeader();
        }

        private int AddIfMissing(List<string> list, string str)
        {
            var id = list.IndexOf(str);
            if (id >= 0)
            {
                return id;
            }

            list.Add(str);
            return list.Count - 1;
        }

        private void XmlToNodeList(XmlNode node)
        {
            var txt = node.ChildNodes.Cast<XmlNode>().FirstOrDefault(i => i is XmlText);

            TreeStructure.Add(new TreeNode(this,
                AddIfMissing(TagList, node.Name),
                AddIfMissing(ValueList, txt?.InnerText ?? "")));

            if (node.Attributes != null)
            {
                foreach (XmlAttribute attrib in node.Attributes)
                {
                    TreeStructure.Add(new TreeNode(this,
                        -AddIfMissing(AttributeList, attrib.Name),
                        AddIfMissing(ValueList, attrib.InnerText)));
                }
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child is XmlElement)
                {
                    XmlToNodeList(child);
                }
            }

            TreeStructure.Add(new TreeNode(this, AddIfMissing(TagList, node.Name), 0xFFFF));
        }

        public string GetXmlString()
        {
            return System.Xml.Linq.XElement.Parse(GetXmlDocument().OuterXml).ToString();
        }
        public XmlDocument GetXmlDocument()
        {
            var doc = new XmlDocument();

            XmlNode node = doc;
            foreach (var entry in TreeStructure!)
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
                    var attributeName = entry.Name!;
                    var attributeValue = entry.Value!;
                    XmlElement el = (XmlElement)node;
                    el.SetAttribute(attributeName, attributeValue);
                }
                else
                {
                    var n = doc.CreateNode(XmlNodeType.Element, entry.Name!, null);
                    n.InnerText = entry.Value!;
                    node.AppendChild(n);
                    node = n;
                }
            }

            return doc;
        }

        private void UpdateHeader()
        {
            TreeStructureCount = TreeStructure.Count;

            TagListCount = TagList.Count;
            TagListOffset = TreeStructure.Count * 4 + 0x28;

            AttributeListCount = AttributeList.Count;
            AttributeListOffset = TagListOffset + TagList.ToBytes(NodeDictEncoding, true).Length;

            ValueListCount = ValueList.Count;
            ValueListOffset = AttributeListOffset + AttributeList.ToBytes(StringDictEncoding, true).Length;
        }

        public override string ToString()
        {
            return GetXmlString();
        }

        //null terminated XBF string
        //0x58_42_46_00
        [Order(-1)]
        [ExpectedValue<string>("XBF")]
        [NullTerminated]
        [BinaryField(Offset = 4 * 0)]
        public string Magic { get; private set; } = "XBF";

        //some secret number
        [Order(-1)]
        [ExpectedValue<int>(0x03_00_80_00)]
        [BinaryFieldAttribute(Offset = 4 * 1)]
        public int Magic2 { get; private set; } = 0x03_00_80_00;

        [ExpectedValue<int>(0x28)]
        [BinaryFieldAttribute(Offset = 4 * 2)] //should be after header
        public int TreeStructureOffset { get; private set; } = 0x28;
        [BinaryFieldAttribute(Offset = 4 * 3)]
        public int TreeStructureCount { get; private set; }
        public int TreeStructureLength => 4 * TreeStructureCount;

        [BinaryFieldAttribute(Offset = 4 * 4)]
        public int TagListOffset { get; private set; }
        [BinaryFieldAttribute(Offset = 4 * 5)]
        public int TagListCount { get; private set; }

        [BinaryFieldAttribute(Offset = 4 * 6)]
        public int AttributeListOffset { get; private set; }
        [BinaryFieldAttribute(Offset = 4 * 7)]
        public int AttributeListCount { get; private set; }

        [BinaryFieldAttribute(Offset = 4 * 8)]
        public int ValueListOffset { get; private set; }
        [BinaryFieldAttribute(Offset = 4 * 9)]
        public int ValueListCount { get; private set; }

        [Order(1)]
        [BinaryFieldAttribute()]
        public List<TreeNode>? TreeStructure { get; private set; } = new List<TreeNode>();

        //three series of null delimited string lists starting after Tree
        //original XBF's lists always have empty string at the begining
        //TODO test if empty string its needed or is just artifact from whatever serialzier was used
        [Order(2)]
        [NullTerminated]
        [BinaryField(OffsetZone = OffsetZone.Absolute)]
        public List<string>? TagList { get; private set; } = [""];
        [Order(3)]
        [NullTerminated]
        [BinaryField(OffsetZone = OffsetZone.Absolute)]
        public List<string>? AttributeList { get; private set; } = [""];
        [Order(4)]
        [NullTerminated]
        [BinaryField(OffsetZone = OffsetZone.Absolute)]
        public List<string>? ValueList { get; private set; } = [""];

        public Encoding NodeDictEncoding { get; }
        public Encoding AttributeDictEncoding { get; }
        public Encoding StringDictEncoding { get; }
    }
}
