using InMemoryBinaryFile;
using InMemoryBinaryFile.Helpers;
using System.Xml;

namespace XBFLib
{
    public class XbfRootSegment : ParentBinarySegment<IBinarySegment, _BaseBinarySegment<XbfRootSegment>>
    {
        const int headerLength = 4 + 4 * 2 * 4;
        const string magicNumber = "XBF\0";
        public XbfRootSegment(bool isUTF8 = false) : base(null, magicNumber, headerLength)
        {
            IsUTF8 = isUTF8;
        }

        public XbfRootSegment(XmlDocument doc, bool isUTF8 = false) : this(isUTF8)
        {
            var allNodes = doc.SelectNodes("//*");
            if (allNodes == null)
            {
                throw new Exception("Mallformed XML");
            }
            var nodes = allNodes.Cast<XmlNode>();

            NodeDict = new XbfStringListSegment(this, nodes.Select(i => i.Name));

            var attributeNodes = doc.SelectNodes("//*/@*")!.Cast<XmlAttribute>();
            AttributeDict = new XbfStringListSegment(this, attributeNodes.Select(i => i.Name));

            var texts = ExtractXbfStringsFromXml(doc.DocumentElement).Distinct().ToList();
            StringDict = new XbfStringListSegment(this, texts);

            NodeTree = new XbfNodeTreeSegment(this, doc);

            UpdateHeaderByBody();
        }

        private IEnumerable<string> ExtractXbfStringsFromXml(XmlNode node)
        {
            List<string> result = new List<string>();
            if (node.ChildNodes != null)
            {
                foreach (XmlNode child in node.ChildNodes)
                {
                    if (child.NodeType == XmlNodeType.Text)
                    {
                        result.Add(child.Value);
                    }
                }
            }
            if (node.Attributes != null)
            {
                foreach (XmlAttribute attr in node.Attributes)
                {
                    result.Add(attr.Value);
                }
            }
            if (node.ChildNodes != null)
            {
                foreach (XmlNode child in node.ChildNodes)
                {
                    if (child.NodeType == XmlNodeType.Element)
                    {
                        result.AddRange(ExtractXbfStringsFromXml(child));
                    }
                }
            }
            return result;
        }

        private void UpdateHeaderByBody()
        {
            HeaderUnknownA = 0x03_00_80_00;

            TreePosition = headerLength + magicNumber.Length;
            TreeLength = NodeTree.GetBytes().Count() / 4;

            NodeDictPosition = TreePosition + TreeLength * 4;
            NodeDictLength = NodeDict.Values.Count;

            AttributeDictPosition = NodeDictPosition + NodeDict.GetBytes().Count();
            AttributeDictLength = AttributeDict.Values.Count;

            StringDictPosition = AttributeDictPosition + AttributeDict.GetBytes().Count();
            StringDictLength = StringDict.Values.Count;
        }

        public XbfNodeTreeSegment NodeTree { get; private set; }
        public XbfStringListSegment NodeDict { get; private set; }
        public XbfStringListSegment AttributeDict { get; private set; }
        public XbfStringListSegment StringDict { get; private set; }
        protected override void ParseBody(Span<byte> body)
        {
            int bodyOffset = headerLength + magicNumber.Length;
            NodeTree = new XbfNodeTreeSegment(this);
            NodeDict = new XbfStringListSegment(this);
            AttributeDict = new XbfStringListSegment(this);
            StringDict = new XbfStringListSegment(this);

            NodeDict.Parse(body.Slice(NodeDictPosition - bodyOffset, AttributeDictPosition - NodeDictPosition));
            AttributeDict.Parse(body.Slice(AttributeDictPosition - bodyOffset, StringDictPosition - AttributeDictPosition));
            StringDict.Parse(body.Slice(StringDictPosition - bodyOffset));

            //relies on dicts located at end of file
            NodeTree.Parse(body.Slice(TreePosition - bodyOffset, TreeLength * 4));
        }

        protected override List<_BaseBinarySegment<XbfRootSegment>> children => new List<_BaseBinarySegment<XbfRootSegment>>()
        {
            NodeTree, NodeDict, AttributeDict, StringDict,
        };

        protected override IEnumerable<byte> HeaderBytes => Concatenate(
            HeaderUnknownA.GetBigEndianBytes(),

            TreePosition.GetBigEndianBytes(),
            TreeLength.GetBigEndianBytes(),

            NodeDictPosition.GetBigEndianBytes(),
            NodeDictLength.GetBigEndianBytes(),

            AttributeDictPosition.GetBigEndianBytes(),
            AttributeDictLength.GetBigEndianBytes(),

            StringDictPosition.GetBigEndianBytes(),
            StringDictLength.GetBigEndianBytes()
            );

        protected override void ParseHeader(Span<byte> header)
        {
            if (header[0x03] != 0)
            {
                throw new Exception($"Unknown value at 0x03. Expected: null. Actual: {header[0x03]}dec {header[0x03]:X2}");
            }

            int dwordCount = 0;

            HeaderUnknownA = header.GetBigEndianDWORD(4 * dwordCount++);

            TreePosition = header.GetBigEndianDWORD(4 * dwordCount++);
            TreeLength = header.GetBigEndianDWORD(4 * dwordCount++);

            NodeDictPosition = header.GetBigEndianDWORD(4 * dwordCount++);
            NodeDictLength = header.GetBigEndianDWORD(4 * dwordCount++);

            AttributeDictPosition = header.GetBigEndianDWORD(4 * dwordCount++);
            AttributeDictLength = header.GetBigEndianDWORD(4 * dwordCount++);

            StringDictPosition = header.GetBigEndianDWORD(4 * dwordCount++);
            StringDictLength = header.GetBigEndianDWORD(4 * dwordCount++);

            if (HeaderUnknownA != 0x03_00_80_00)
            {
                throw new Exception($"Unknown value of HeaderUnknownA {HeaderUnknownA}dec {HeaderUnknownA:X8}");
                throw new Exception($"Unknown value of HeaderUnknownA. Expected: 0x03_00_80_00. Actual: {HeaderUnknownA}dec {HeaderUnknownA:X8}");
            }
        }

        public int HeaderUnknownA { get; private set; }

        public int TreePosition { get; private set; }
        public int TreeLength { get; private set; }
        public int NodeDictPosition { get; private set; }
        public int NodeDictLength { get; private set; }
        public int AttributeDictPosition { get; private set; }
        public int AttributeDictLength { get; private set; }
        public int StringDictPosition { get; private set; }
        public int StringDictLength { get; private set; }
        public bool IsUTF8 { get; private set; }
    }
}