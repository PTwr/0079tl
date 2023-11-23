using InMemoryBinaryFile.New.Attributes;

namespace GEVLib.New
{
    [BinarySegment(BodyOffset = 4)]
    public class STRSegment : InMemoryBinaryFile.New.IBinarySegment
    {
        [FixedLengthString(ExpectedValue = "$STR", Offset = 4 * 0, Order = -1, Length = 4, OffsetScope = OffsetScope.Segment)]
        public string Magic { get; private set; } = "$STR";

        [NullTerminatedStringAttribute(CodePage = 932, Offset = 0, Alignment = 4, OffsetScope = OffsetScope.Segment, OffsetZone = OffsetZone.Body)]
        public Dictionary<int, string> STR { get; private set; }
    }
}
