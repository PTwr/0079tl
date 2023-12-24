using GEVLib.EVE;
using InMemoryBinaryFile.New;
using InMemoryBinaryFile.New.Attributes;

namespace GEVLib.New
{
    [BinarySegment]
    public class EVEBlock : _BaseBinarySegment<EVESegment>
    {
        public EVEBlock(EVESegment parent) : base(parent)
        {
        }

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

        public bool IsJumpTable()
        {
            return Lines.Count == 1
                && Lines.First().IsJumpTable();
        }

        public int JumpOffset
        {
            get
            {
                var blockPos = this.Parent.Blocks.IndexOf(this);
                var prevBlocks = this.Parent.Blocks.Take(blockPos);
                var blockOffset = prevBlocks.Sum(i=> i.SegmentLength);

                //offset to 32bit chunks
                return blockOffset / 4;
            }
        }
    }
}
