using InMemoryBinaryFile.Helpers;
using InMemoryBinaryFile.New.Attributes;

namespace GEVLib.New
{
    [BinarySegment(Length = 4)]
    public class EVEOpCode : InMemoryBinaryFile.New.IBinarySegment
    {
        public EVEOpCode()
        {
            
        }

        public EVEOpCode(ushort command, ushort parameter)
        {
            this.Command = command;
            this.Parameter = parameter;
        }
        public EVEOpCode(uint opcode)
        {
            this.Command = opcode.GetHighUWORD();
            this.Parameter = opcode.GetLowUWORD();
        }

        [BinaryField(Offset = 0, OffsetScope = OffsetScope.Segment, OffsetZone = OffsetZone.Body)]
        public ushort Command { get; private set; }
        [BinaryField(Offset = 2, OffsetScope = OffsetScope.Segment, OffsetZone = OffsetZone.Body)]
        public ushort Parameter { get; private set; }

        public override string ToString()
        {
            return $"0x{Command:X4}{Parameter:X4}";
        }
    }
}
