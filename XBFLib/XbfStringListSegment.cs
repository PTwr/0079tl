using InMemoryBinaryFile;
using InMemoryBinaryFile.Helpers;
using System.Text;

namespace XBFLib
{
    public class XbfStringListSegment : StringListSegment<XbfRootSegment>
    {
        public XbfStringListSegment(XbfRootSegment parent, Encoding encoding) : base(parent, encoding)
        {
        }

        public XbfStringListSegment(XbfRootSegment parent, IEnumerable<string> values, Encoding encoding, bool deduplicate = true) : base(parent, values, encoding, deduplicate)
        {
        }
    }
}