using GEVLib.OFS;
using InMemoryBinaryFile.New;
using InMemoryBinaryFile.New.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEVLib.New
{
    [BinarySegment(HeaderOffset = 8, BodyOffset = 0x1C)]
    public class GEVFile : InMemoryBinaryFile.New.IBinarySegment, IPostProcessing
    {
        [FixedLengthString(ExpectedValue = "$EVFEV02", Offset = 4 * 0, Order = -1, Length = 8)]
        public string Magic { get; private set; } = "$EVFEV02";


        [BinaryFieldAttribute(Offset = 4 * 0, OffsetZone = OffsetZone.Header)]
        public int EVEBlockCount { get; private set; }
        [BinaryFieldAttribute(Offset = 4 * 1, OffsetZone = OffsetZone.Header, ExpectedValue = 0x20)]
        public int Alignment { get; private set; }
        [BinaryFieldAttribute(Offset = 4 * 2, OffsetZone = OffsetZone.Header)]
        public int OFSDataCount { get; private set; }
        [BinaryFieldAttribute(Offset = 4 * 3, OffsetZone = OffsetZone.Header)]
        public int OFSDataOffset { get; private set; }
        [BinaryFieldAttribute(Offset = 4 * 4, OffsetZone = OffsetZone.Header)]
        public int STRDataOffset { get; private set; }

        //TODO complete EVE Section
        [BinaryField(Order = 2, Offset = 0, OffsetZone = OffsetZone.Body)]
        public EVESegment EVESegment { get; private set; }
        public int EVESegmentLength => (OFSDataOffset - 4 - 0x1C); //between header and $OFS

        [BinaryField]
        public OFSSegment OFSSegment { get; private set; }
        public int OFSSegmentOffset => OFSDataOffset - 4;
        public bool OFSSegmentIf => OFSDataOffset > 0;

        [BinaryField]
        public STRSegment STRSegment { get; private set; }
        public int STRSegmentOffset => STRDataOffset - 4;
        public bool STRSegmentIf => STRDataOffset > 0;

        public GEVFile()
        {

        }

        public void AfterDeserialization()
        {
            Validate();
        }
        public void Validate()
        {
            if (OFSDataOffset > 0 && STRDataOffset == 0)
                throw new NotSupportedException("OFS without STR is not supported");
            if (OFSDataOffset == 0 && STRDataOffset > 0)
                throw new NotSupportedException("OFS without STR is not supported");

            var ofsLength = (OFSBinarySegment.magicNumber.Length + OFSDataCount * 2);
            ofsLength += ofsLength % (Alignment / 8);
            if (OFSDataOffset > 0 && (STRDataOffset - OFSDataOffset) != ofsLength)
                throw new NotSupportedException("OFS length mismatch");
        }
    }
}
