using InMemoryBinaryFile.New.Attributes;

namespace GEVLib.New
{
    [BinarySegment(Length = 4)]
    public class EVEOpCode : InMemoryBinaryFile.New.IBinarySegment
    {
        [BinaryField(Offset = 0, OffsetScope = OffsetScope.Segment, OffsetZone = OffsetZone.Body)]
        public ushort Command { get; private set; }
        [BinaryField(Offset = 2, OffsetScope = OffsetScope.Segment, OffsetZone = OffsetZone.Body)]
        public ushort Parameter { get; private set; }
    }
}
