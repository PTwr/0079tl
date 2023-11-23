using InMemoryBinaryFile.New.Attributes;

namespace GEVLib.New
{
    [BinarySegment(BodyOffset = 4)]
    public class OFSSegment : InMemoryBinaryFile.New._BaseBinarySegment<GEVFile>
    {
        public OFSSegment(GEVFile parent) : base(parent)
        {
        }

        [FixedLengthString(ExpectedValue = "$OFS", Offset = 4 * 0, Order = -1, Length = 4, OffsetScope = OffsetScope.Segment)]
        public string Magic { get; private set; } = "$OFS";

        [BinaryField(Offset = 0, OffsetScope = OffsetScope.Segment, OffsetZone = OffsetZone.Body)]
        public List<OFSEntry> OFSEntries { get; private set; }
        public int OFSEntriesCount => this.Parent.OFSDataCount;
    }
}
