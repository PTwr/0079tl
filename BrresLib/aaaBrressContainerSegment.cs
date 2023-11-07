//using InMemoryBinaryFile;
//using InMemoryBinaryFile.Helpers;

//namespace BrresLib
//{
//    public class BrressContainerSegment : ParentBinarySegment<IBinarySegment, _BaseBinarySegment<BrressContainerSegment>>
//    {
//        public const string magicNumber = "bres";
//        const int headerLength = 2 + 2 + 4 + 2 + 2;
//        public BrressContainerSegment() : base(null, magicNumber.ToASCIIBytes(), headerLength)
//        {
//        }

//        protected override void ParseBody(Span<byte> body, Span<byte> everything)
//        {
//            rootNode = new BrressRootNodeSegment(this);
//            rootNode.Parse(body, everything);
//        }

//        protected override List<_BaseBinarySegment<BrressContainerSegment>> children => new List<_BaseBinarySegment<BrressContainerSegment>> { rootNode };

//        protected override void ParseHeader(Span<byte> header, Span<byte> everything)
//        {
//            byteOrderMark = header.GetBigEndianUWORD(0);
//            padding = header.GetBigEndianUWORD(2);
//            length = header.GetBigEndianUWORD(4);
//            rootOffset = header.GetBigEndianUWORD(8);
//            sectionCount = header.GetBigEndianUWORD(10);
//        }
//        BrressRootNodeSegment rootNode = null;
//        ushort byteOrderMark;
//        ushort padding;
//        uint length;
//        ushort rootOffset;
//        ushort sectionCount;
//    }
//    public class BrressRootNodeSegment : ParentBinarySegment<BrressContainerSegment, _BaseBinarySegment<BrressRootNodeSegment>>
//    {
//        public const string magicNumber = "root";
//        const int headerLength = 4+4;
//        public BrressRootNodeSegment(BrressContainerSegment? parent) : base(parent, magicNumber.ToASCIIBytes(), headerLength)
//        {
//        }

//        protected override void ParseBody(Span<byte> body, Span<byte> everything)
//        {
//            SubNodes = new List<BrressIndexNodeSegment>();
//            indexGroup = new BrressIndexNodeSegment(this);
//            indexGroup.Parse(body, everything);
//        }

//        List<BrressIndexNodeSegment> SubNodes = new List<BrressIndexNodeSegment>();
//        protected override List<_BaseBinarySegment<BrressRootNodeSegment>> children => new List<_BaseBinarySegment<BrressRootNodeSegment>>(SubNodes) { indexGroup };

//        protected override void ParseHeader(Span<byte> header, Span<byte> everything)
//        {
//            size = header.GetBigEndianUDWORD(0);
//        }
//        BrressIndexNodeSegment indexGroup = null;
//        uint size;
//    }
//    public class BrressIndexNodeSegment : ParentBinarySegment<BrressRootNodeSegment, _BaseBinarySegment<BrressIndexNodeSegment>>
//    {
//        public const string magicNumber = "";
//        const int headerLength = 4+4;
//        public BrressIndexNodeSegment(BrressRootNodeSegment? parent) : base(parent, magicNumber.ToASCIIBytes(), headerLength)
//        {
//        }

//        protected override void ParseBody(Span<byte> body, Span<byte> everything)
//        {
//            var offset = 0x10; //16b file header
//            offset += 0x08; //root node header
//            //throw new NotImplementedException();
//            for (int i=0;i<entryCount;i++)
//            {
//                var dataEnd = offset + 0x10; //?????

//                var groupentry = new BrresGroupEntry(this, offset, dataEnd);

//                offset += 0x10;                
//            }
//        }

//        protected override void ParseHeader(Span<byte> header, Span<byte> everything)
//        {
//            size = header.GetBigEndianUDWORD(0);
//            entryCount = header.GetBigEndianUDWORD(4);
//        }
//        uint size;
//        uint entryCount;
//    }

//    public class BrresGroupEntry : ParentBinarySegment<BrressIndexNodeSegment, _BaseBinarySegment<BrresGroupEntry>>
//    {
//        public const string magicNumber = "";
//        const int headerLength = 4*2+2*4;
//        public BrresGroupEntry(BrressIndexNodeSegment? parent, int dataStart, int dataEnd) : base(parent, magicNumber.ToASCIIBytes(), headerLength)
//        {
//            DataStart = dataStart;
//            DataEnd = dataEnd;
//        }

//        protected override void ParseBody(Span<byte> body, Span<byte> everything)
//        {
//            var nameLength = everything[(int)name_offset - 1]; //pascal string?
//            var nameBytes = everything.Slice((int)name_offset, nameLength); //but utf8?
//            var name = nameBytes.ToUTF8String();

//            var data = everything.Slice(DataStart, DataEnd - DataStart);

//        }

//        protected override void ParseHeader(Span<byte> header, Span<byte> everything)
//        {
//            id = header.GetBigEndianUWORD(0);
//            unknown = header.GetBigEndianUWORD(2);
//            left_index = header.GetBigEndianUWORD(4);
//            right_index = header.GetBigEndianUWORD(6);
//            name_offset = header.GetBigEndianUWORD(10);
//            data_offset = header.GetBigEndianUWORD(12);
//        }

//        ushort id;
//        ushort unknown;
//        ushort left_index;
//        ushort right_index;
//        uint name_offset;
//        uint data_offset;

//        public int DataStart { get; }
//        public int DataEnd { get; }
//    }
//}