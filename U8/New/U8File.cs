using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InMemoryBinaryFile.Helpers;
using InMemoryBinaryFile.New;
using InMemoryBinaryFile.New.Attributes;
using InMemoryBinaryFile.New.Serialization;
using XBFLib;
using XBFLib.New;

namespace U8.New
{
    [BinarySegmentAttribute(HeaderOffset = 4, BodyOffset = 0x20)]
    public class U8File : InMemoryBinaryFile.New.IBinarySegment
    {
        [BinaryFieldAttribute(ExpectedValue = 0x55_AA_38_2D, Offset = 0, Order = -1)]
        public int Magic { get; set; }

        //should be same as BodyOffset
        [BinaryFieldAttribute(ExpectedValue = 0x20, Offset = 0, OffsetZone = OffsetZone.Header, Order = 0)]
        public int RootNodeOffset { get; set; }
        [BinaryFieldAttribute(Offset = 4, OffsetZone = OffsetZone.Header, Order = 0)]
        public int NodeListAndStringDictLength { get; set; }
        [BinaryFieldAttribute(Offset = 8, OffsetZone = OffsetZone.Header, Order = 0)]
        public int DataOffset { get; set; }

        //32 bytes of zeros
        [BinaryFieldAttribute(ExpectedValue = 0x00_00_00_00, Offset = 12, OffsetZone = OffsetZone.Header, Order = 0)]
        public int ZerosA { get; set; }
        [BinaryFieldAttribute(ExpectedValue = 0x00_00_00_00, Offset = 16, OffsetZone = OffsetZone.Header, Order = 0)]
        public int ZerosB { get; set; }
        [BinaryFieldAttribute(ExpectedValue = 0x00_00_00_00, Offset = 20, OffsetZone = OffsetZone.Header, Order = 0)]
        public int ZerosC { get; set; }
        [BinaryFieldAttribute(ExpectedValue = 0x00_00_00_00, Offset = 24, OffsetZone = OffsetZone.Header, Order = 0)]
        public int ZerosD { get; set; }

        [BinaryFieldAttribute(Offset = 8, OffsetZone = OffsetZone.Body, Order = 0)]
        public int NodeListCount { get; set; }

        [NullTerminatedString(OffsetZone = OffsetZone.Body, Order = 2)]
        public Dictionary<int, string>? FileNames { get; set; }
        public int FileNamesOffset => NodeListCount * 12;
        public int FileNamesLength => NodeListAndStringDictLength - FileNamesOffset;

        //[BinaryFieldAttribute(Offset = 0, OffsetZone = OffsetZone.Body, Order = 3)]
        //public List<U8Node> NodeList { get; set; }

        [BinaryFieldAttribute(Offset = 0, OffsetZone = OffsetZone.Body, Order = 5)]
        public U8HierarchicalNode? U8HierarchicalNode { get; set; }
        public int U8HierarchicalNodeLength => NodeListCount * 12;
    }

    [BinarySegmentAttribute(BodyOffset = 12)]
    public class U8HierarchicalNode : U8Node, IPostProcessing
    {
        public IEnumerable<U8HierarchicalNode> Items()
        {
            yield return this;
            foreach(var item in U8HierarchicalNodes.SelectMany(i=>i.Items()))
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

        public U8HierarchicalNode? Root => (Parent is U8File) ? this : ((Parent as U8HierarchicalNode).Root);

        public int NodeCounter;

        public int Id { get; private set; }

        [BinaryFieldAttribute(Offset = 0, OffsetZone = OffsetZone.Body, OffsetScope = OffsetScope.Segment, Order = 20)]
        public List<U8HierarchicalNode>? U8HierarchicalNodes { get; set; }
        public bool U8HierarchicalNodesIf => IsDirectory;
        public int U8HierarchicalNodesLength => (LastNodeId - Id) * 12;

        public int SegmentLength => IsFile ? 12 : (U8HierarchicalNodesLength + 12);

        public void AfterDeserialization()
        {
        }

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
    public class U8Node : InMemoryBinaryFile.New.BinarySegment<IBinarySegment>
    {
        public U8Node(IBinarySegment parent) : base(parent)
        {
        }

        public U8File? File => (Parent as U8File) ?? (Parent as U8Node)?.File ?? null;

        //counting from 1, id of node thats last child of this ndoe
        public int LastNodeId => IsDirectory ? BinaryDataLength : 0;

        public bool IsFile => this.Type == 0x00;
        public bool IsDirectory => this.Type == 0x01;
        public bool IsArc => IsFile && BinaryData.AsSpan().StartsWith(U8RootSegment.U8MagicNumber);
        public bool IsXbf => IsFile && BinaryData.AsSpan().StartsWith(XbfRootSegment.magicNumber.ToASCIIBytes());

        [BinaryFieldAttribute(Offset = 0, OffsetZone = OffsetZone.Header, OffsetScope = OffsetScope.Segment, Order = 0)]
        public byte Type { get; set; } //this is really a u8
        //TODO implement (U)Int24 reading
        [BinaryFieldAttribute(Offset = 2, OffsetZone = OffsetZone.Header, OffsetScope = OffsetScope.Segment, Order = 0)]
        public short NameOffset { get; set; } //really a "u24", byte #1 is ignored in this implementation


        [BinaryFieldAttribute(Offset = 4, OffsetZone = OffsetZone.Header, OffsetScope = OffsetScope.Segment, Order = 0)]
        public int BinaryDataOffset { get; set; }
        [BinaryFieldAttribute(Offset = 8, OffsetZone = OffsetZone.Header, OffsetScope = OffsetScope.Segment, Order = 0)]
        public int BinaryDataLength { get; set; }
        [BinaryFieldAttribute(OffsetZone = OffsetZone.Absolute, OffsetScope = OffsetScope.Absolute, Order = 1)]
        public byte[] BinaryData { get; set; }
        public bool BinaryDataIf => IsFile;

        public string Name => File.FileNames[NameOffset];

        public override string ToString()
        {
            return Name;
        }

        public XbfFile? XbfFile => IsXbf ? Deserializer.Deserialize<XbfFile>(BinaryData.AsSpan()) : null;
        public U8File? U8File => IsArc ? Deserializer.Deserialize<U8File>(BinaryData.AsSpan()) : null;
    }

}
