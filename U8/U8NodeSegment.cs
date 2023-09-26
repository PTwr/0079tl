using InMemoryBinaryFile;
using InMemoryBinaryFile.Helpers;
using XBFLib;

namespace U8
{
    public class U8NodeSegment : ParentBinarySegment<IBinarySegment, U8NodeSegment>
    {
        public U8NodeSegment(IBinarySegment? parent) : base(parent, headerLength: 12)
        {
        }

        protected override void ParseBody(Span<byte> body)
        {
            //body is located in shared data segment, at DataOffset
        }

        protected override void ParseHeader(Span<byte> header)
        {
            Type = header[0];

            var u24firstByte = header[1];
            if (u24firstByte != 0)
            {
                throw new Exception("NameOffset is u24 value, but values larger than u16 are not currently supported");
            }

            NameOffset = header.GetBigEndianWORD(2);
            DataOffset = header.GetBigEndianDWORD(4);
            Size = header.GetBigEndianDWORD(8);
        }

        public bool IsFile => this.Type == 0x00;
        public bool IsDirectory => this.Type == 0x01;
        public bool IsArc => IsFile && BinaryData.AsSpan().StartsWith(U8RootSegment.U8MagicNumber);
        public bool IsXbf => IsFile && BinaryData.AsSpan().StartsWith(XbfRootSegment.magicNumber.ToASCIIBytes());

        public byte Type { get; private set; } //this is really a u8
        public short NameOffset { get; private set; } //really a "u24"
        public int DataOffset { get; /*private*/ set; }
        public int Size { get; /*private*/ set; }

        public string Name { get; set; }
        public string Path { get; set; }
        public byte[] BinaryData { get; set; }

        public override string ToString()
        {
            return Path ?? Name ?? base.ToString() ?? "";
        }

        public override IEnumerable<byte> GetBytes()
        {
            return Concatenate(
                new byte[] { Type, 0 },
                NameOffset.GetBigEndianBytes(),
                DataOffset.GetBigEndianBytes(),
                Size.GetBigEndianBytes()
                );
        }
    }
}
