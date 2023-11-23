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

        [FixedLengthString(ExpectedValue = "$OFS", Order = 1, Length = 4)]
        public string OFSMagic { get; private set; } = "$OFS";
        public int OFSMagicOffset => OFSDataOffset - 4;
        public bool OFSMagicIf => OFSDataOffset > 0;

        [FixedLengthString(ExpectedValue = "$STR", Order = 1, Length = 4)]
        public string STRMagic { get; private set; } = "$STR";
        public int STRMagicOffset => STRDataOffset - 4;
        public bool STRMagicIf => STRDataOffset > 0;

        [FixedLengthString(ExpectedValue = "$EVE", Order = 1, Length = 4, Offset = 0, OffsetZone = OffsetZone.Body)]
        public string EVEMagic { get; private set; } = "$EVE";

        //TODO complete EVE Section
        [BinaryField(Order = 2, Offset = 4, OffsetZone = OffsetZone.Body)]
        public EVESegment EVESegment { get; private set; }
        public int EVESegmentLength => (OFSMagicOffset - 0x1C); //between header and $OFS

        [BinaryField(Order = 2, Offset = 4, OffsetZone = OffsetZone.Body)]
        public List<EVEOpCode> EVEOpCodes { get; private set; }
        public int EVEOpCodesCount => EVESegmentLength / 4; //4 bytes per opcode

        //read data directly, without nested objects
        [NullTerminatedStringAttribute(CodePage = 932, Alignment = 4, Order = 2)]
        public Dictionary<int, string> STRData { get; private set; }
        public bool STRDataIf => STRDataOffset > 0;

        //nested helper objects
        [BinaryField]
        public OFSSegment OFSSegment { get; private set; }
        public int OFSSegmentOffset => OFSDataOffset - 4;
        public bool OFSSegmentIf => OFSDataOffset > 0;

        //OFSSegment.OFSEntries, but without wrapper class
        //TODO add (de)serialization of collections of primitive types, can't deserialize List<ushort> currently
        [BinaryField(Order = 2)]
        public List<OFSEntry> OFSData { get; private set; }
        public bool OFSDataIf => OFSDataOffset > 0;

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

    [BinarySegment]
    public class EVESegment : IBinarySegment
    {

    }
}
