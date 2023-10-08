using GEVLib.EVE;
using GEVLib.OFS;
using GEVLib.STR;
using InMemoryBinaryFile;
using InMemoryBinaryFile.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEVLib.GEV
{
    public class GEVBinaryRootSegment : ParentBinarySegment<IBinarySegment, _BaseBinarySegment<GEVBinaryRootSegment>>
    {
        public const string magicNumber = "$EVFEV02";
        const int headerLength = 5 * 4;
        public GEVBinaryRootSegment() : base(null, magicNumber.ToASCIIBytes(), headerLength)
        {
        }

        public void UpdateHeader()
        {
            //TODO EVE

            OFSStart = 0;
            STRStart = 0;

            if (OFS != null)
            {
                OFSWordLength = OFS.StringIndexes.Count();
                OFSStart = this.MagicNumber.Length + this.HeaderLength + this.EVE.GetBytes().Count();
                OFSStart += OFSBinarySegment.magicNumber.Length;
            }

            if (OFS != null && STR != null)
            {
                STRStart = OFSStart + this.OFS.GetBytes().Count();
                STRStart += OFSBinarySegment.magicNumber.Length;
            }
        }

        protected override void ParseHeader(Span<byte> header, Span<byte> everything)
        {
            EVEBlockCount = header.GetBigEndianDWORD(0);
            Alignment = header.GetBigEndianDWORD(4); //TODO check if it affects STR or just sections, is it bits (4B str alignment) or bytes?
            OFSWordLength = header.GetBigEndianDWORD(8);
            OFSStart = header.GetBigEndianDWORD(12);
            STRStart = header.GetBigEndianDWORD(16);

            if (OFSStart > 0 && STRStart == 0)
                throw new NotSupportedException("OFS without STR is not supported");
            if (OFSStart == 0 && STRStart > 0)
                throw new NotSupportedException("OFS without STR is not supported");

            var ofsLength = (OFSBinarySegment.magicNumber.Length + OFSWordLength * 2);
            ofsLength += ofsLength % (Alignment / 8);
            if (OFSStart > 0 && (STRStart - OFSStart) != ofsLength)
                throw new NotSupportedException("OFS length mismatch");
        }

        protected override void ParseBody(Span<byte> body, Span<byte> everything)
        {
            //0x1bb4 0x1c28
            EVE = new EVEBinarySegment(this);
            var evebytes = body
                .Slice(0, OFSStart - OFSBinarySegment.magicNumber.Length - this.HeaderLength - this.MagicNumber.Length);
            EVE.Parse(evebytes);

            if (OFSStart > 0)
            {
                OFS = new OFSBinarySegment(this);
                var ofsbytes = everything.Slice(
                    OFSStart - OFSBinarySegment.magicNumber.Length,
                    OFSBinarySegment.magicNumber.Length + OFSWordLength * 4
                    );
                OFS.Parse(ofsbytes);
            }

            if (STRStart > 0)
            {
                STR = new STRBinarySegment(this);
                var strbytes = everything.Slice(
                    STRStart - OFSBinarySegment.magicNumber.Length
                    );
                STR.Parse(strbytes);
            }
        }

        protected override IEnumerable<byte> HeaderBytes => Concatenate(
            EVEBlockCount.GetBigEndianBytes(),
            Alignment.GetBigEndianBytes(),
            OFSWordLength.GetBigEndianBytes(),
            OFSStart.GetBigEndianBytes(),
            STRStart.GetBigEndianBytes()
            );

        public int EVEBlockCount { get; private set; }
        public int Alignment { get; private set; }
        public int OFSWordLength { get; private set; }
        public int OFSStart { get; private set; }
        public int STRStart { get; private set; }

        protected override List<_BaseBinarySegment<GEVBinaryRootSegment>> children
        {
            get
            {
                var result = new List<_BaseBinarySegment<GEVBinaryRootSegment>> { EVE };
                if (OFS != null) result.Add(OFS);
                if (STR != null) result.Add(STR);
                return result;
            }
        }

        public EVEBinarySegment EVE { get; private set; }
        public OFSBinarySegment? OFS { get; private set; }
        public STRBinarySegment? STR { get; private set; }
    }
}
