using InMemoryBinaryFile.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryBinaryFile
{
    public class StringDictSegment<TParent> : _BaseBinarySegment<TParent>
        where TParent : IBinarySegment
    {
        private Dictionary<int, string> values = new Dictionary<int, string>();
        public StringDictSegment(TParent parent, Encoding encoding) : base(parent)
        {
            Encoding = encoding;
        }

        public string this[int i]
        {
            get { return values[i]; }
        }

        public IReadOnlyList<string> Values => values.Values.ToList().AsReadOnly();

        protected override void ParseBody(Span<byte> body)
        {
            for (int start = 0; start < body.Length;)
            {
                //series of null terminated strings
                var s = body.Slice(start).FindNullTerminator();
                var ss = s.ToDecodedString(Encoding);
                if (string.IsNullOrEmpty(ss))
                {
                    //TODO special handling for nulls?
                    //ss = "(empty string)";
                }
                values[start] = ss;

                start += s.Length + 1;
            }
        }

        protected override void ParseHeader(Span<byte> header)
        {
        }

        protected override IEnumerable<byte> BodyBytes => values.Values.SelectMany(s => s.ToBytes(Encoding, appendNullTerminator: true));

        public Encoding Encoding { get; }
    }
}
