﻿using InMemoryBinaryFile.New;
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
        public int TerminatorOpCodeOffset => this.Parent.OFSMagicOffset - 4;

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

    [BinarySegment]
    public class EVEBlock : IBinarySegment
    {
        //TODO remember offset for JumpTable calcs

        [BinaryField(Offset = 0, OffsetScope = OffsetScope.Segment, OffsetZone = OffsetZone.Absolute, Order = 1)]
        public List<EVELine> Lines { get; private set; }
        //stop reading after last line
        //I HATE IT
        public bool LinesContinue(List<IBinarySegment> items) => !(items.Cast<EVELine>().LastOrDefault()?.IsLastLine == true);

        [BinaryField(OffsetScope = OffsetScope.Segment, OffsetZone = OffsetZone.Absolute, Order = 10)]
        public EVEOpCode ReturnOpCode { get; private set; } = new EVEOpCode(0x0005FFFF);
        public int ReturnOpCodeOffset => Lines.Sum(i => i.SegmentLength);

        public int SegmentLength => Lines.Sum(i => i.SegmentLength) + 4;

        public override string ToString()
        {
            return string.Join(Environment.NewLine, Lines.Select(i => i.ToString()));
        }
    }

    [BinarySegment]
    public class EVELine : IBinarySegment
    {
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
    }
}
