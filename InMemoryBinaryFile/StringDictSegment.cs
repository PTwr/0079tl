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
        public StringDictSegment(TParent parent, Encoding encoding, byte[]? magicNumber = null, int alignment = 1) : base(parent, magicNumber)
        {
            Encoding = encoding;
            Alignment = alignment;
        }

        public string this[int i]
        {
            get { return values[i]; }
        }

        public IReadOnlyList<string> Strings => values.Values.ToList().AsReadOnly();
        public IReadOnlyDictionary<int, string> IndexedStrings => values.AsReadOnly();

        protected override void ParseBody(Span<byte> body, Span<byte> everything)
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

                //start = start + Alignment - start % Alignment;

                start = (int)(Math.Ceiling(1.0 * start / Alignment) * Alignment);

            }
        }

        public void ReplaceStrings(List<string> strings)
        {
            if (strings.Count() != values.Count())
            {
                throw new NotSupportedException("Adding/Removing strings is not supported yet");
            }

            values.Clear();

            int pos = 0;
            for (int i = 0; i < strings.Count(); i++)
            {
                var bytes = strings[i].ToBytes(Encoding, appendNullTerminator: true).PadToAlignment(Alignment);
                values[pos] = strings[i];

                pos += bytes.Count();
            }
        }

        protected override void ParseHeader(Span<byte> header, Span<byte> everything)
        {
        }

        protected override IEnumerable<byte> BodyBytes => values.Values.SelectMany(s => s.ToBytes(Encoding, appendNullTerminator: true).PadToAlignment(Alignment));

        public Encoding Encoding { get; }
        public int Alignment { get; }

        public override string ToString()
        {
            if (Alignment != 0)
            {
                return string.Join(Environment.NewLine, values.Select(i => $"{i.Key:X8} {i.Key / Alignment:X8} {i.Value}"));
            }
            return string.Join(Environment.NewLine, values.Select(i => $"{i.Key:X8} {i.Value}"));
        }
    }
}
