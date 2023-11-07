using InMemoryBinaryFile;
using InMemoryBinaryFile.Helpers;
using System.Xml;

namespace XBFLib
{
    public class XbfNodeTreeSegment : _BaseBinarySegment<XbfRootSegment>
    {
        public XmlDocument XmlDocument { get; private set; } = new XmlDocument();
        public XbfNodeTreeSegment(XbfRootSegment parent) : base(parent)
        {

        }

        public XbfNodeTreeSegment(XbfRootSegment parent, XmlDocument doc) : this(parent)
        {
            this.XmlDocument = doc;
        }

        public override string ToString()
        {
            return System.Xml.Linq.XElement.Parse(XmlDocument.OuterXml).ToString();
        }

        protected override IEnumerable<byte> BodyBytes
        {
            get
            {
                var nodeNames = this.Parent.NodeDict.Values.Select((s, i) => new { s, i }).ToDictionary(x => x.s, x => (short)x.i);
                var nodeValues = this.Parent.StringDict.Values.Select((s, i) => new { s, i }).ToDictionary(x => x.s, x => (short)x.i);
                var nodeAttributes = this.Parent.AttributeDict.Values.Select((s, i) => new { s, i }).ToDictionary(x => x.s, x => (short)(-x.i));

                var bytes = NodeToBytes(XmlDocument.DocumentElement, nodeNames, nodeValues, nodeAttributes);

                return bytes.ToArray();
            }
        }

        private IEnumerable<byte> NodeToBytes(XmlNode node, Dictionary<string, short> names, Dictionary<string, short> values, Dictionary<string, short> attributes)
        {
            var txt = node.SelectNodes("./text()")!.Cast<XmlNode>().FirstOrDefault()?.InnerText ?? "";
            var start = Concatenate(names[node.Name].GetBigEndianBytes(), values[txt].GetBigEndianBytes());

            var attr = node.Attributes.Cast<XmlAttribute>()
                .SelectMany(a => Concatenate(attributes[a.Name].GetBigEndianBytes(), values[a.Value].GetBigEndianBytes()));

            var inner = node.ChildNodes.Cast<XmlNode>()
                .Where(n => n.NodeType == XmlNodeType.Element)
                .SelectMany(n => NodeToBytes(n, names, values, attributes))
                .ToList();

            var stop = Concatenate(names[node.Name].GetBigEndianBytes(), new byte[] { 0xFF, 0xFF });

            return Concatenate(start, attr, inner, stop);
        }

        protected override void ParseBody(Span<byte> body, Span<byte> everything)
        {
            var nodeNames = this.Parent.NodeDict.Values;
            var nodeTexts = this.Parent.StringDict.Values;
            var nodeAttributes = this.Parent.AttributeDict.Values;

            XmlNode node = XmlDocument;

            var sss = body.ToArray().ToHexString(lineLength: 4);

            int pos = 0;
            while (pos < body.Length)
            {
                var nodeNameId = body.GetBigEndianWORD(pos);
                var nodeValueId = body.GetBigEndianUWORD(pos + 2);

                if (nodeValueId == 0xFFFF) // </node>
                {
                    if (node.ParentNode == null)
                    {
                        throw new Exception($"Error in XML tree. Parent of {node.Name} is null.");
                    }
                    node = node.ParentNode;
                }
                else if (nodeNameId < 0)
                {
                    var attributeName = nodeAttributes[nodeNameId*-1];
                    XmlElement el = (XmlElement)node;
                    el.SetAttribute(attributeName, nodeTexts[nodeValueId]);
                }
                else
                {
                    var n = XmlDocument.CreateNode(XmlNodeType.Element, nodeNames[nodeNameId], null);
                    n.InnerText = nodeTexts[nodeValueId];
                    node.AppendChild(n);
                    node = n;
                }

                pos += 4;
            }
        }

        protected override void ParseHeader(Span<byte> header, Span<byte> everything)
        {
        }
    }
}