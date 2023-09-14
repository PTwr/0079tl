using InMemoryBinaryFile;
using InMemoryBinaryFile.Helpers;

namespace XBFLib
{
    public class XbfStringListSegment : _BaseBinarySegment<XbfRootSegment>
    {
        private List<string> values = new List<string>();
        public XbfStringListSegment(XbfRootSegment parent) : base(parent)
        {
        }
        public XbfStringListSegment(XbfRootSegment parent, IEnumerable<string> values) : this(parent)
        {
            this.values = new List<string>(values
                .Where(i => i != null && i != "") //empty string has special handling
                .Distinct()); //XBF deduplicates values

            //ensure empty string is at first slot
            this.values.Insert(0, "");
        }

        public IReadOnlyList<string> Values => values.AsReadOnly();

        protected override void ParseBody(Span<byte> body)
        {
            for (int start = 0; start < body.Length;)
            {
                //series of null terminated strings
                var s = body.Slice(start).FindNullTerminator();
                start += s.Length + 1;
                var ss = Parent.IsUTF8 ? s.ToUTF8String() : s.ToW1250String();
                if (string.IsNullOrEmpty(ss))
                {
                    //TODO special handling for nulls?
                    //ss = "(empty string)";
                }
                values.Add(ss);
            }
        }

        protected override void ParseHeader(Span<byte> header)
        {
        }

        protected override IEnumerable<byte> BodyBytes => values.SelectMany(s => Parent.IsUTF8 ? s.ToUTF8Bytes(appendNullTerminator: true) : s.ToW1250Bytes(appendNullTerminator: true));
    }
}