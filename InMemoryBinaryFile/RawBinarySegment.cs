using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryBinaryFile
{
    public class RawBinarySegment<TParent> : _BaseBinarySegment<TParent>
        where TParent : IBinarySegment
    {
        private IEnumerable<byte> body = Enumerable.Empty<byte>();

        public RawBinarySegment(TParent parent, Span<byte> content, byte[]? magicNumber = null, int headerLength = 0) : base(parent, magicNumber, headerLength)
        {
        }

        protected override void ParseBody(Span<byte> body, Span<byte> everything)
        {
            this.body = body.ToArray();
        }

        protected override void ParseHeader(Span<byte> header, Span<byte> everything)
        {
        }


        protected override IEnumerable<byte> BodyBytes => body;
    }
}
