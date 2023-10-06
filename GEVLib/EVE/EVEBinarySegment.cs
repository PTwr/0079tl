using GEVLib.GEV;
using InMemoryBinaryFile;
using InMemoryBinaryFile.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEVLib.EVE
{
    public class EVEBinarySegment : ParentBinarySegment<GEVBinaryRootSegment, _BaseBinarySegment<EVEBinarySegment>>
    {
        public const string magicNumber = "$EVE";
        const int headerLength = 0;
        public EVEBinarySegment(GEVBinaryRootSegment Parent) : base(Parent, magicNumber.ToASCIIBytes(), headerLength)
        {
        }

        byte[] data;
        protected override void ParseBody(Span<byte> body, Span<byte> everything)
        {
            data = body.ToArray();
        }

        protected override void ParseHeader(Span<byte> header, Span<byte> everything)
        {
            //throw new NotImplementedException();
        }

        protected override IEnumerable<byte> BodyBytes => data;
    }
}
