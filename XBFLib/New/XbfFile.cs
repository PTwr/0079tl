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

        public TreeNode(XbfFile parent, int nameOrAttributeId, int valueId) : base(parent)
        {
            NameOrAttributeId = (short)nameOrAttributeId;
            ValueId = (ushort)valueId;
        }

        //NodeDict if positive (XmlNode name) or AttributeDict if negative (XmlAttribute name)
        [BinaryFieldAttribute(Offset = 0, OffsetScope = OffsetScope.Segment)]
        public short NameOrAttributeId { get; private set; }

        //StringDict (XmlNode/XmlAttributes value) unless IsClosingTag (0xFFFF)
        [BinaryFieldAttribute(Offset = 2, OffsetScope = OffsetScope.Segment)]
        public ushort ValueId { get; private set; }

        public bool IsClosingTag => ValueId == 0xFFFF;
        public bool IsAttribute => NameOrAttributeId < 0;

        public string? Name => IsClosingTag ? null : (IsAttribute ? 
            Parent!.AttributeList!.ElementAt(NameOrAttributeId * -1) : 
            Parent!.NodeList!.ElementAt(NameOrAttributeId));

        public string? Value => IsClosingTag ? null : Parent!.StringList!.ElementAt(ValueId);

        public override string ToString()
        {
            return IsClosingTag ? "</>" : IsAttribute ? $"<... {Name}=\"{Value}\"" : $"<{Name}>{Value}";
        }

    }

    [BinarySegment(HeaderOffset = 8, BodyOffset = 0x28)]
    public class XbfFile : InMemoryBinaryFile.New.IBinarySegment
    {
        public XbfFile() : this(Encoding.UTF8)
        {
            
        }

        public XbfFile(Encoding encoding)
        {
            NodeDictEncoding = AttributeDictEncoding = StringDictEncoding = encoding;
        }

        public XbfFile(XmlDocument doc, Encoding? encoding = null) : this(encoding ?? Encoding.UTF8)
        {
            //TODO fill tree and dicts from xml

            TraverseXml(doc.FirstChild);

            UpdateHeader();
        }

        private int AddIfMissing(List<string> list, string str)
        {
            var id = list.IndexOf(str);
            if (id>=0)
            {
                return id;
            }

            list.Add(str);
            return list.Count - 1;
        }

        private void TraverseXml(XmlNode node)
        {
            var txt = node.ChildNodes.Cast<XmlNode>().FirstOrDefault(i => i is XmlText);

            Tree.Add(new TreeNode(this,
                AddIfMissing(NodeList, node.Name),
                AddIfMissing(StringList, txt?.InnerText ?? "")));

            if (node.Attributes != null)
            {
                foreach (XmlAttribute attrib in node.Attributes)
                {
                    Tree.Add(new TreeNode(this,
                        -AddIfMissing(AttributeList, node.Name),
                        AddIfMissing(StringList, node.InnerText)));
                }
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child is XmlElement)
                {
                    TraverseXml(child);
                }
            }

            Tree.Add(new TreeNode(this, 0, 0xFFFF));
        }

        public string GetXmlString()
        {
            return System.Xml.Linq.XElement.Parse(GetXmlDocument().OuterXml).ToString();
        }
        public XmlDocument GetXmlDocument()
        {
            var doc = new XmlDocument();

            XmlNode node = doc;
            foreach (var entry in Tree!)
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
            TreeCount = Tree.Count;

            NodeListCount = NodeList.Count;
            NodeListOffset = Tree.Count * 12 + 0x28;

            AttributeListCount = AttributeList.Count;
            AttributeListOffset = NodeListOffset + NodeList.ToBytes(NodeDictEncoding, true).Length;

            StringListCount = AttributeList.Count;
            StringListOffset = AttributeListOffset + AttributeList.ToBytes(StringDictEncoding, true).Length;
        }

        public override string ToString()
        {
            return GetXmlString();
        }

        //null terminated XBF string
        [BinaryFieldAttribute(ExpectedValue = 0x58_42_46_00, Offset = 4 * 0, Order = -1)]
        public int Magic { get; private set; } = 0x58_42_46_00;

        //some secret number
        [BinaryFieldAttribute(ExpectedValue = 0x03_00_80_00, Offset = 4 * 1, Order = -1)]
        public int Magic2 { get; private set; } = 0x03_00_80_00;

        [BinaryFieldAttribute(Offset = 4 * 2, ExpectedValue = 0x28)] //should be after header
        public int TreeOffset { get; private set; } = 0x28;
        [BinaryFieldAttribute(Offset = 4 * 3)]
        public int TreeCount { get; private set; }
        public int TreeLength => 4 * TreeCount;

        [BinaryFieldAttribute(Offset = 4 * 4)]
        public int NodeListOffset { get; private set; }
        [BinaryFieldAttribute(Offset = 4 * 5)]
        public int NodeListCount { get; private set; }

        [BinaryFieldAttribute(Offset = 4 * 6)]
        public int AttributeListOffset { get; private set; }
        [BinaryFieldAttribute(Offset = 4 * 7)]
        public int AttributeListCount { get; private set; }

        [BinaryFieldAttribute(Offset = 4 * 8)]
        public int StringListOffset { get; private set; }
        [BinaryFieldAttribute(Offset = 4 * 9)]
        public int StringListCount { get; private set; }

        [BinaryFieldAttribute(Order = 1)]
        public List<TreeNode>? Tree { get; private set; } = new List<TreeNode>();

        //three series of null delimited string lists starting after Tree
        //original XBF's lists always have empty string at the begining
        //TODO test if empty string its needed or is just artifact from whatever serialzier was used
        [NullTerminatedString(Order = 2, OffsetZone = OffsetZone.Absolute)]
        public List<string>? NodeList { get; private set; } = [""];
        [NullTerminatedString(Order = 3, OffsetZone = OffsetZone.Absolute)]
        public List<string>? AttributeList { get; private set; } = [""];
        [NullTerminatedString(Order = 4, OffsetZone = OffsetZone.Absolute)]
        public List<string>? StringList { get; private set; } = [""];

        public Encoding NodeDictEncoding { get; }
        public Encoding AttributeDictEncoding { get; }
        public Encoding StringDictEncoding { get; }
    }
}
