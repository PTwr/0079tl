using BinarySerializer;
using BinarySerializer.Annotation;
using XBFLib;

namespace U8.NewNew
{
    [BinarySegmentAttribute(HeaderOffset = 4, BodyOffset = 0x20)]
    public class U8File : IBinarySegment
    {
        public const int MagicNumber = 0x55_AA_38_2D;

        [Order(-1)]
        [ExpectedValue<int>(MagicNumber)]
        [BinaryField(Offset = 0)]
        public int Magic { get; set; }

        //should be same as BodyOffset
        [ExpectedValue<int>(0x20)]
        [BinaryField(Offset = 0, OffsetZone = OffsetZone.Header)]
        public int RootNodeOffset { get; set; }

        //NodeListLength + FileNamesLength
        [BinaryField(Offset = 4, OffsetZone = OffsetZone.Header)]
        public int ContentTreeDetailsLength { get; set; }

        [BinaryField(Offset = 8, OffsetZone = OffsetZone.Header)]
        public int DataOffset { get; set; }

        [ExpectedValue<int[]>([0, 0, 0, 0])]
        [Collection(Count = 4)]
        [BinaryField(Offset = 12, OffsetZone = OffsetZone.Header)]
        public int[] Zeros { get; set; } = [0, 0, 0, 0];

        [BinaryField(Offset = 8, OffsetZone = OffsetZone.Body)]
        public int NodeListCount { get; set; }

        [Order(2)]
        [NullTerminated]
        [BinaryField(OffsetZone = OffsetZone.Body)]
        public Dictionary<int, string>? FileNames { get; set; }
        public int FileNamesOffset => NodeListCount * 12;
        public int FileNamesLength => ContentTreeDetailsLength - FileNamesOffset;

        [Order(5)]
        [BinaryField(Offset = 0, OffsetZone = OffsetZone.Body)]
        public U8DirectoryNode? RootNode { get; set; }
        public int U8TreeLength => NodeListCount * 12;

        //HACK deserialization is a more or less stateless and nodes are deserialized recursively so we abuse root object to store node counter
        internal int NodeCounter = 0;

        public U8Node this[string name] => RootNode[name];
    }

    public class U8DirectoryNode : U8Node
    {
        public U8DirectoryNode(IBinarySegment parent) : base(parent)
        {
        }

        [Order(int.MaxValue)] //deserialize after Offset/Length/IsFile
        [DeserializeAs<U8DirectoryNode>(IfStartsWithPattern = [0x01])]
        [DeserializeAs<U8FileNode>(IfStartsWithPattern = [0x00])]
        [BinaryField(Offset = 0, OffsetZone = OffsetZone.Body)]
        public List<U8Node> Children { get; set; }
        public bool ChildrenContinue(IEnumerable<U8Node> list)
        {
            //HACK read children until specified nodeId is reached
            return RootFile.NodeCounter < LastNodeId;
        }

        [BinaryField(Offset = 8, OffsetZone = OffsetZone.Header)]
        public int LastNodeId { get; set; }

        //override base Length of 12 to also encompass all children
        public int SegmentLength => (LastNodeId - Id) * 12 + 12;

        public override U8Node this[string name] => Children.FirstOrDefault(node => node.Name == name);
    }

    public class U8FileNode : U8Node
    {
        public U8FileNode(IBinarySegment parent) : base(parent)
        {
        }

        [Order(int.MaxValue)] //deserialize after Offset/Length/IsFile
        [NestedFile]
        //absolute offset from file begining
        [BinaryField(OffsetZone = OffsetZone.Absolute, OffsetScope = OffsetScope.Absolute)]
        [DeserializeAs<XbfFile>(XbfFile.MagicNumber)]
        [DeserializeAs<U8File>(U8File.MagicNumber)]
        public IBinarySegment Content { get; set; }

        [BinaryFieldAttribute(Offset = 4, OffsetZone = OffsetZone.Header)]
        public int ContentOffset { get; set; }
        [BinaryFieldAttribute(Offset = 8, OffsetZone = OffsetZone.Header)]
        public int ContentLength { get; set; }

        public XbfFile? Xbf => Content as XbfFile;
        public U8File? U8 => Content as U8File;
        public RawBinarySegment? Raw => Content as RawBinarySegment;

        public override U8Node this[string name]
        {
            get
            {
                switch(Content)
                {
                    case U8File u8:
                        return u8[name];
                    default:
                        throw new Exception($"Not a traversable content type '{Content.GetType().FullName}'");
                }
            }
        }
    }


    [BinarySegment(BodyOffset = 12, Length = 12)]
    public abstract class U8Node : _BaseBinarySegment<IBinarySegment>
    {
        public int Id { get; set; }
        public U8Node(IBinarySegment parent) : base(parent)
        {
            //HACK abuse root container to store deserialization state
            if (Parent is U8File)
            {
                //root node always has Id=1
                RootFile.NodeCounter = Id = 1;
            }
            else
            {
                //nested nodes have lineary incremental Id
                Id = ++RootFile.NodeCounter;
            }
        }

        [BinaryField(Offset = 0)]
        public byte Type { get; set; } //this is really a u8

        public bool IsFile => this.Type == 0x00;
        public bool IsDirectory => this.Type == 0x01;

        //TODO implement (U)Int24 reading
        [BinaryField(Offset = 2)]
        public short NameOffset { get; set; } //really a "u24", byte #1 is ignored in this implementation

        //recursively look for root object
        public U8File? RootFile => (Parent as U8File) ?? (Parent as U8Node)?.RootFile ?? null;

        public string Name => RootFile.FileNames[NameOffset];

        //recursively build path
        private string GetPath(string separator = "/")
        {
            if (Parent is U8File)
            {
                return Name;
            }
            return ((U8Node)Parent!).GetPath() + separator + Name;
        }

        public override string ToString()
        {
            return GetPath();
        }

        public abstract U8Node this[string name] { get; }
    }
}
