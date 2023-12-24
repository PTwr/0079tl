using GEVLib.EVE;
using InMemoryBinaryFile.New;
using InMemoryBinaryFile.New.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace GEVLib.New
{
    [BinarySegment]
    public class EVELine : _BaseBinarySegment<EVEBlock>
    {
        public EVELine(EVEBlock parent) : base(parent)
        {
        }

        public override string ToString()
        {
            return $"Id: 0x{LineId:X4} Count: 0x{LineOpCodeCount:X4} Unknown: 0x{LineOpCodeCount_Unknown}"
                + Environment.NewLine
                + string.Join(Environment.NewLine, EVEOpCodes.Select(i => "  " + i.ToString()));
        }

        //TODO fix deserialzier to not crash on mismatched types for matched values
        [BinaryField(ExpectedValue = (ushort)0x0001, Offset = 0, OffsetScope = OffsetScope.Segment, OffsetZone = OffsetZone.Absolute)]
        public ushort LineStartOpCode { get; private set; }
        [BinaryField(Offset = 2, OffsetScope = OffsetScope.Segment, OffsetZone = OffsetZone.Absolute)]
        public ushort LineId { get; private set; }

        [BinaryField(Offset = 4, OffsetScope = OffsetScope.Segment, OffsetZone = OffsetZone.Absolute)]
        public ushort LineOpCodeCount { get; private set; }
        [BinaryField(Offset = 6, OffsetScope = OffsetScope.Segment, OffsetZone = OffsetZone.Absolute)]
        public ushort LineOpCodeCount_Unknown { get; private set; } //line type?

        [BinaryField(ExpectedValue = (uint)0x0004_0000, OffsetScope = OffsetScope.Segment, OffsetZone = OffsetZone.Absolute, Order = 10)]
        public uint LineTerminator { get; private set; }
        public int LineTerminatorOffset => 4 * LineOpCodeCount - 4;

        [BinaryField(OffsetScope = OffsetScope.Segment, OffsetZone = OffsetZone.Absolute, Order = 10, Serialize = false)]
        public uint OpcodeLookahead { get; private set; }
        public int OpcodeLookaheadOffset => 4 * LineOpCodeCount;

        public bool IsLastLine => OpcodeLookahead == 0x0005FFFF; //0x0005FFFF return jump rather than line start 0x0001{nnnn}

        [BinaryField(Offset = 0, OffsetScope = OffsetScope.Segment, OffsetZone = OffsetZone.Absolute, Order = 100)]
        public List<EVEOpCode> EVEOpCodes { get; private set; }
        public int EVEOpCodesCount => LineOpCodeCount;

        public int SegmentLength => 4 * LineOpCodeCount;

        public bool IsJumpTable()
        {
            return EVELine_JumpTable.Is(this);
        }
    }

    public class EVELine_JumpTable
    {
        public static bool Is(EVELine line)
        {
            return line.EVEOpCodesCount > 4
                && line.EVEOpCodes[2].DWORD == EVEConsts.EVECommandStartDword
                && line.EVEOpCodes[3].Command == EVEConsts.JumpTableEndPointer;
        }

        public EVELine_JumpTable(EVELine line)
        {
            if (!Is(line))
            {
                throw new Exception("Not a JumpTable EVE Line");
            }

            Line = line;

            if (JumpTableEnd - 1 != Line.LineOpCodeCount)
            {
                throw new NotSupportedException("Jump table is expected to fully fill code line");
            }
        }

        public EVELine Line { get; private set; }
        public ushort JumpTableEnd => Line.EVEOpCodes[3].Parameter;

        public int JumpCount => (JumpTableEnd - 2) / 2;

        public List<JumpTableEntry> JumpTableEntries { get; } = new List<JumpTableEntry>();

        public void ParseJumps()
        {
            var blocksByOffset = Line.Parent.Parent.Blocks
                .ToDictionary(i => i.JumpOffset, i => i);

            for (int i = 4; i < Line.EVEOpCodes.Count - 1; i += 2)
            {
                var jumpCode = Line.EVEOpCodes[i];
                var returnLabel = Line.EVEOpCodes[i + 1];

                if (jumpCode.Command != EVEConsts.JumpCode
                    || returnLabel.Parameter != EVEConsts.PaddingCode)
                {
                    throw new InvalidDataException("Not a jump table entry");
                }

                JumpTableEntries.Add(new JumpTableEntry()
                {
                    ReturnLabel = returnLabel.Parameter,
                    EVEBlock = blocksByOffset[jumpCode.Command]
                });
            }
        }

        public class JumpTableEntry()
        {
            public EVEBlock EVEBlock { get; set; }
            public ushort ReturnLabel { get; set; }
        }
    }
}
