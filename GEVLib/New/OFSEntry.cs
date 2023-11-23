using InMemoryBinaryFile.New;
using InMemoryBinaryFile.New.Attributes;

namespace GEVLib.New
{
    //length is just Id, string is fetched from another segment
    [BinarySegment(Length = 2)]
    public class OFSEntry : InMemoryBinaryFile.New._BaseBinarySegment<IBinarySegment>
    {
        public OFSEntry(IBinarySegment parent) : base(parent)
        {
        }

        [BinaryField(Offset = 0, OffsetScope = OffsetScope.Segment, Order = -1)]
        public ushort ValueDWORDOffset { get; private set; }

        [NullTerminatedString(CodePage = 932)]
        public string Value { get; private set; }
        public int ValueOffset => Root.STRDataOffset + ValueDWORDOffset * 4;

        public override string ToString()
        {
            return Value;
        }

        private GEVFile Root => (this.Parent as GEVFile) ?? (this.Parent as OFSSegment).Parent;
    }
}
