using InMemoryBinaryFile.New;
using InMemoryBinaryFile.New.Attributes;

namespace XBFLib.New
{
    [BinarySegment(HeaderOffset = 0, BodyOffset = 0, Length = 4)]
    public class TreeNode : _BaseBinarySegment<XbfFile>
    {
        public TreeNode(XbfFile parent) : base(parent)
        {
        }

        public TreeNode(XbfFile parent, int nameOrAttributeId, int valueId) : base(parent)
        {
            NameOrAttributeId = (short)nameOrAttributeId;
            ValueId = (ushort)valueId;
        }

        //NodeDict if positive (XmlNode name) or AttributeDict if negative (XmlAttribute name)
        [BinaryFieldAttribute(Offset = 0, OffsetScope = OffsetScope.Segment)]
        public short NameOrAttributeId { get; private set; }

        //StringDict (XmlNode/XmlAttributes value) unless IsClosingTag (0xFFFF)
        [BinaryFieldAttribute(Offset = 2, OffsetScope = OffsetScope.Segment)]
        public ushort ValueId { get; private set; }

        public bool IsClosingTag => ValueId == 0xFFFF;
        public bool IsAttribute => NameOrAttributeId < 0;

        public string? Name => IsAttribute ? 
            Parent!.AttributeList!.ElementAt(NameOrAttributeId * -1) : 
            Parent!.TagList!.ElementAt(NameOrAttributeId);

        public string? Value => IsClosingTag ? null : Parent!.ValueList!.ElementAt(ValueId);

        public override string ToString()
        {
            return IsClosingTag ? $"</{Name}>" : IsAttribute ? $"<... {Name}=\"{Value}\"" : $"<{Name}>{Value}";
        }

    }
}
