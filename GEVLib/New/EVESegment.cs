using InMemoryBinaryFile.New;
using InMemoryBinaryFile.New.Attributes;

namespace GEVLib.New
{
    [BinarySegment]
    public class EVESegment : _BaseBinarySegment<GEVFile>
    {
        public EVESegment(GEVFile parent) : base(parent)
        {
        }

        [FixedLengthString(ExpectedValue = "$EVE", Order = 1, Length = 4, Offset = 0, OffsetScope = OffsetScope.Segment)]
        public string EVEMagic { get; private set; } = "$EVE";

        //TODO add comparer primitive-object, if object has implicit/explicit cast?
        [BinaryField]
        public EVEOpCode TerminatorOpCode { get; private set; } = new EVEOpCode(0x0006FFFF);
        public int TerminatorOpCodeOffset => this.Parent.OFSDataOffset - 4 - 4;

        [BinaryField(ExpectedValue = (uint)0x0006FFFF)]
        public uint TerminatorDWORD { get; private set; }
        public int TerminatorDWORDOffset => TerminatorOpCodeOffset;


        [BinaryField(Offset = 4, OffsetScope = OffsetScope.Segment, OffsetZone = OffsetZone.Absolute, Order = 1)]
        public List<EVEBlock> Blocks { get; private set; }
        public int BlocksLength => SegmentLength - 4;

        public int SegmentLength => this.Parent.EVESegmentLength-4; //Blocks.Sum(i => i.SegmentLength) + 4;

        public override string ToString()
        {
            return string.Join(Environment.NewLine, Blocks.Select(i => i.ToString() + Environment.NewLine + "---------------------------------------------------"));
        }
    }
}
