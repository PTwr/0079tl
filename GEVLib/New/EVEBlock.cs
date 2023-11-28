using InMemoryBinaryFile.New;
using InMemoryBinaryFile.New.Attributes;

namespace GEVLib.New
{
    [BinarySegment]
    public class EVEBlock : IBinarySegment
    {
        //TODO remember offset for JumpTable calcs

        [BinaryField(Offset = 0, OffsetScope = OffsetScope.Segment, OffsetZone = OffsetZone.Absolute, Order = 1)]
        public List<EVELine> Lines { get; private set; }
        //stop reading after last line
        //I HATE IT
        public bool LinesContinue(List<IBinarySegment> items) => !(items.Cast<EVELine>().LastOrDefault()?.IsLastLine == true);

        [BinaryField(OffsetScope = OffsetScope.Segment, OffsetZone = OffsetZone.Absolute, Order = 10)]
        public EVEOpCode ReturnOpCode { get; private set; } = new EVEOpCode(0x0005FFFF);
        public int ReturnOpCodeOffset => Lines.Sum(i => i.SegmentLength);

        public int SegmentLength => Lines.Sum(i => i.SegmentLength) + 4;

        public override string ToString()
        {
            return string.Join(Environment.NewLine, Lines.Select(i => i.ToString()));
        }
    }
}
