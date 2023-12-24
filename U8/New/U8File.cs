using BinarySerializer;
using BinarySerializer.Helpers;
using BinarySerializer.Annotation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XBFLib;

namespace U8.New
{
    [BinarySegmentAttribute(HeaderOffset = 4, BodyOffset = 0x20)]
    public class U8File : IBinarySegment
    {
        [Order(-1)]
        [ExpectedValue<int>(0x55_AA_38_2D)]
        [BinaryFieldAttribute(Offset = 0)]
        public int Magic { get; set; }

        //should be same as BodyOffset
        [ExpectedValue<int>(0x20)]
        [BinaryFieldAttribute(Offset = 0, OffsetZone = OffsetZone.Header)]
        public int RootNodeOffset { get; set; }
        [BinaryFieldAttribute(Offset = 4, OffsetZone = OffsetZone.Header)]
        public int NodeListAndStringDictLength { get; set; }
        [BinaryFieldAttribute(Offset = 8, OffsetZone = OffsetZone.Header)]
        public int DataOffset { get; set; }

        [ExpectedValue<int[]>([0, 0, 0, 0])]
        [Collection(Count = 4)]
        [BinaryFieldAttribute(Offset = 12, OffsetZone = OffsetZone.Header)]
        public int[] Zeros { get; set; }

        //32 bytes of zeros
        //[ExpectedValue<int>(0x00_00_00_00)]
        //[BinaryFieldAttribute(Offset = 12, OffsetZone = OffsetZone.Header)]
        //public int ZerosA { get; set; }
        //[ExpectedValue<int>(0x00_00_00_00)]
        //[BinaryFieldAttribute(Offset = 16, OffsetZone = OffsetZone.Header)]
        //public int ZerosB { get; set; }
        //[ExpectedValue<int>(0x00_00_00_00)]
        //[BinaryFieldAttribute(Offset = 20, OffsetZone = OffsetZone.Header)]
        //public int ZerosC { get; set; }
        //[ExpectedValue<int>(0x00_00_00_00)]
        //[BinaryFieldAttribute(Offset = 24, OffsetZone = OffsetZone.Header)]
        //public int ZerosD { get; set; }

        [BinaryFieldAttribute(Offset = 8, OffsetZone = OffsetZone.Body)]
        public int NodeListCount { get; set; }

        [Order(2)]
        [NullTerminated]
        [BinaryField(OffsetZone = OffsetZone.Body)]
        public Dictionary<int, string>? FileNames { get; set; }
        public int FileNamesOffset => NodeListCount * 12;
        public int FileNamesLength => NodeListAndStringDictLength - FileNamesOffset;

        //[BinaryFieldAttribute(Offset = 0, OffsetZone = OffsetZone.Body, Order = 3)]
        //public List<U8Node> NodeList { get; set; }

        [Order(5)]
        [BinaryFieldAttribute(Offset = 0, OffsetZone = OffsetZone.Body)]
        public U8HierarchicalNode? U8Tree { get; set; }
        public int U8TreeLength => NodeListCount * 12;
    }

    [BinarySegmentAttribute(BodyOffset = 12)]
    public class U8HierarchicalNode : U8Node
    {
        public IEnumerable<U8HierarchicalNode> Items()
        {
            yield return this;
            foreach (var item in U8HierarchicalNodes.SelectMany(i => i.Items()))
            {
                yield return item;
            }
        }

        public U8HierarchicalNode(IBinarySegment parent) : base(parent)
        {
            if (Parent is U8File)
            {
                //root node always has Id=1
                NodeCounter = Id = 1;
            }
            else
            {
                //nested nodes have lineary incremental Id
                Id = ++Root.NodeCounter;
            }
        }

        public void RecalculateLastNodeId()
        {
            throw new NotImplementedException();

            //LastNodeId = nodes.SelectMany(indepth:true).Count();
            //TODO recalculate binary offsets as well

            //TODO add option to place modified binary data in "extra offset",
            //for nicer Riivolution partial patch (only header and new data will be changed that way)
            //don't forget to byteallign to 4, to mathc riivolution offsets allignment
        }

        public U8HierarchicalNode? Root => (Parent is U8File) ? this : ((Parent as U8HierarchicalNode).Root);

        public int NodeCounter;

        public int Id { get; private set; }

        [Order(20)]
        [BinaryFieldAttribute(Offset = 0, OffsetZone = OffsetZone.Body)]
        public List<U8HierarchicalNode>? U8HierarchicalNodes { get; set; }
        public bool U8HierarchicalNodesIf => IsDirectory;
        public int U8HierarchicalNodesLength => (LastNodeId - Id) * 12;

        public int SegmentLength => IsFile ? 12 : (U8HierarchicalNodesLength + 12);

        private string GetPath(string separator = "/")
        {
            if (Parent is U8File)
            {
                return Name;
            }
            return ((U8HierarchicalNode)Parent!).GetPath() + separator + Name;
        }

        public override string ToString()
        {
            return GetPath();
        }
    }

    [BinarySegmentAttribute(Length = 12)]
    public class U8Node : _BaseBinarySegment<IBinarySegment>
    {
        public U8Node(IBinarySegment parent) : base(parent)
        {
        }

        public U8File? File => (Parent as U8File) ?? (Parent as U8Node)?.File ?? null;

        //counting from 1, id of node thats last child of this ndoe
        public int LastNodeId => IsDirectory ? BinaryDataLength : 0;

        public bool IsFile => this.Type == 0x00;
        public bool IsDirectory => this.Type == 0x01;
        public bool IsU8 => U8File != null;
        public bool IsXbf => XbfFile != null;

        [BinaryFieldAttribute(Offset = 0, OffsetZone = OffsetZone.Header)]
        public byte Type { get; set; } //this is really a u8
        //TODO implement (U)Int24 reading
        [BinaryFieldAttribute(Offset = 2, OffsetZone = OffsetZone.Header)]
        public short NameOffset { get; set; } //really a "u24", byte #1 is ignored in this implementation


        [BinaryFieldAttribute(Offset = 4, OffsetZone = OffsetZone.Header)]
        public int BinaryDataOffset { get; set; }
        [BinaryFieldAttribute(Offset = 8, OffsetZone = OffsetZone.Header)]
        public int BinaryDataLength { get; set; }
        [Order(10)]
        [BinaryFieldAttribute(OffsetZone = OffsetZone.Absolute, OffsetScope = OffsetScope.Absolute)]
        public byte[] BinaryData { get; set; }
        //TODO implement exclusive groups instead? Might be useful for Gev codeblocks/lines
        public bool BinaryDataIf => IsFile && !IsU8 && !IsXbf;

        public string Name => File.FileNames[NameOffset];

        public override string ToString()
        {
            return Name;
        }

        [Order(200)]
        [BinaryFieldAttribute(OffsetZone = OffsetZone.Absolute, OffsetScope = OffsetScope.Absolute, SeparateScope = true)]
        [DeserializeAsAttribute<XbfFile>(IfFunc = nameof(IsFile), IfStartsWithPattern = [0x58, 0x42, 0x46, 0x00])]
        [DeserializeAsAttribute<U8File>(IfFunc = nameof(IsFile), IfStartsWithPattern = [0x55, 0xAA, 0x38, 0x2D])]
        [DeserializeAsAttribute<RawBinarySegment>(IfFunc = nameof(IsFile), Order = int.MaxValue)] //fallback
        public IBinarySegment Content { get; set; }
        public int ContentOffset => BinaryDataOffset;
        public int ContentLength => BinaryDataLength;

        [Order(2)]
        [DeserializeAsAttribute<XbfFile>(IfStartsWithPattern = [0x58, 0x42, 0x46, 0x00])]
        //TODO IsXbf/IsArc requries access to binary data, use Span<byte?>.Match extension to automatically detect?
        [BinaryFieldAttribute(OffsetZone = OffsetZone.Absolute, OffsetScope = OffsetScope.Absolute, SeparateScope = true)]
        public XbfFile? XbfFile { get; set; }
        public bool XbfFileIf => !IsDirectory;
        public int XbfFileOffset => BinaryDataOffset;
        public int XbfFileLength => BinaryDataLength;

        [Order(2)]
        [DeserializeAsAttribute<U8File>(IfStartsWithPattern = [0x55, 0xAA, 0x38, 0x2D])]
        [BinaryFieldAttribute(OffsetZone = OffsetZone.Absolute, OffsetScope = OffsetScope.Absolute, SeparateScope = true)]
        public U8File? U8File { get; set; }
        public bool U8FileIf => !IsDirectory;
        public int U8FileOffset => BinaryDataOffset;
        public int U8FileLength => BinaryDataLength;
    }

}
