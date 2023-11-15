using InMemoryBinaryFile.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryBinaryFile
{
    public class StringListSegment<TParent> : _BaseBinarySegment<TParent>
        where TParent: IBinarySegment
    {
        private List<string> values = new List<string>();
        public StringListSegment(TParent parent, Encoding encoding) : base(parent)
        {
            Encoding = encoding;
        }
        public StringListSegment(TParent parent, IEnumerable<string> values, Encoding encoding, bool deduplicate = true) : this(parent, encoding)
        {
            values = values
                .Where(i => i != null && i != ""); //empty string has special handling

            values = deduplicate ? values.Distinct() : values;

            //ensure empty string is at first slot
            this.values.Add("");
            this.values.AddRange(values);
        }

        public IReadOnlyList<string> Values => values.AsReadOnly();

        protected override void ParseBody(Span<byte> body, Span<byte> everything)
        {
            values.AddRange(body.ToDecodedNullTerminatedStrings(Encoding));
        }

        protected override void ParseHeader(Span<byte> header, Span<byte> everything)
        {
        }

        protected override IEnumerable<byte> BodyBytes => values.SelectMany(s => s.ToBytes(Encoding, appendNullTerminator: true));

        public Encoding Encoding { get; }
    }
}
