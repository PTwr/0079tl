using InMemoryBinaryFile;
using InMemoryBinaryFile.Helpers;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Emit;

namespace GEVLib.EVE
{

    public class EVECodeLine : _BaseBinarySegment<EVECodeBlock>
    {
        public const int DefaultHeaderLength = 8;
        public EVECodeLine(EVECodeBlock? parent, int headerLength = DefaultHeaderLength) : base(parent, headerLength: headerLength)
        {
        }

        protected List<uint> OpCodes = new List<uint>();
        protected virtual int OpCodeCount() => LineLength - 2;
        protected override void ParseBody(Span<byte> body, Span<byte> everything)
        {
            OpCodes.Clear();

            var codeOperandsByteCount = OpCodeCount() * 4;
            var codeOperands = body.Slice(0, codeOperandsByteCount);

            for (int i = 0; i < codeOperands.Length; i += 4)
            {
                OpCodes.Add(codeOperands.GetBigEndianUDWORD(i));
            }

            if (OpCodes.Last() != EVEConsts.LineEndDword)
            {
                throw new InvalidDataException($"Invalid lineend operand {OpCodes.Last():X8}");
            }
        }

        public void ReplaceOpcode(uint oldopcode, uint newopcode)
        {
            for (int i = 0; i < OpCodes.Count; i++)
            {
                if (OpCodes[i] == oldopcode)
                {
                    OpCodes[i] = newopcode;
                }
            }
        }
        public void SetOpcode(uint opcode, int n)
        {
            OpCodes[n] = opcode;
        }

        protected override IEnumerable<byte> BodyBytes
        {
            get
            {
                return OpCodes
                    .SelectMany(i => i.GetBigEndianBytes());
            }
        }
        protected override IEnumerable<byte> HeaderBytes => Concatenate(
            LineMarker.GetBigEndianBytes(),
            CodeLineId.GetBigEndianBytes(),
            LineLength.GetBigEndianBytes(),
            Unknown.GetBigEndianBytes()
            );

        protected override void ParseHeader(Span<byte> header, Span<byte> everything)
        {
            LineMarker = header.GetBigEndianUWORD();
            CodeLineId = header.GetBigEndianUWORD(2);

            if (LineMarker != 1)
            {
                throw new Exception("Not an EVE code block");
            }

            LineLength = header.GetBigEndianUWORD(4);
            Unknown = header.GetBigEndianUWORD(6);
        }
        //should be sequential
        public ushort LineMarker { get; private set; }
        public ushort CodeLineId { get; private set; }
        public ushort LineLength { get; protected set; }
        public ushort Unknown { get; private set; }

        public bool IsLastLine { get; set; }

        public override string ToString()
        {
            var str = "";

            for (int i = 0, opcodeN = 3; i < OpCodes.Count; i++, opcodeN++)
            {
                var opcode = OpCodes[i];
                var nextOpCode = i + 1 < OpCodes.Count ? OpCodes[i + 1] : 0;
                var nextnextOpCode = i + 2 < OpCodes.Count ? OpCodes[i + 2] : 0;
                if (opcode.GetHighUWORD() == 0x0050 || opcode.GetHighUWORD() == 0x0056)
                {
                    //6char MS name takes two opcode spaces, +2 padding nulls
                    if (i< OpCodes.Count-2)
                    {
                        var strbytes = nextOpCode.GetBigEndianBytes().Concat(nextnextOpCode.GetBigEndianBytes()).Take(6).ToArray().AsSpan();
                        var mscode = strbytes.ToAsciiString();

                        str += $"[{opcodeN:D2}] mech code: {mscode}" + Environment.NewLine;
                    }
                }
                if (opcode.GetHighUWORD() == 0x0046)
                {
                    var param = opcode.GetLowUWORD();

                    str += $"[{opcodeN:D2}] Conditional check? variable {param:X4} 16b value {nextOpCode:X8}" + Environment.NewLine;
                }
                if (opcode.GetHighUWORD() == 0x0047)
                {
                    var param = opcode.GetLowUWORD();

                    str += $"[{opcodeN:D2}] Conditional set? variable {param:X4} 16b value {nextOpCode:X8}" + Environment.NewLine;

                }
                if (opcode.GetHighUWORD() == EVEConsts.VoiceBlock.Opcode2)
                {
                    var ofsId = opcode.GetLowUWORD();
                    if (this.Parent.Parent.Parent.OFS.StringIndexes.Count > ofsId)
                    {
                        str += $"[{opcodeN:D2}] Blocking playback #{ofsId:X2} {this.Parent.Parent.Parent.OFS.Translated(ofsId)}" + Environment.NewLine;
                    }
                }
                if (opcode.GetHighUWORD() == 0x011B)
                {
                    var ofsId = opcode.GetLowUWORD();
                    if (this.Parent.Parent.Parent.OFS.StringIndexes.Count > ofsId)
                    {
                        str += $"[{opcodeN:D2}] Non-blocking playback #{ofsId:X2} {this.Parent.Parent.Parent.OFS.Translated(ofsId)}" + Environment.NewLine;
                    }
                }
                if (opcode.GetHighUWORD() == 0x00C1)
                {
                    var ofsId = opcode.GetLowUWORD();
                    if (this.Parent.Parent.Parent.OFS.StringIndexes.Count > ofsId)
                    {
                        str += $"[{opcodeN:D2}] Textbox {this.Parent.Parent.Parent.OFS.Translated(ofsId)}" + Environment.NewLine;
                    }
                }
                if (opcode.GetHighUWORD() == 0x0011)
                {
                    var jump = opcode.GetLowUWORD();
                    var jumptable = this.Parent.Parent.Parent.EVE.Children.First().Children.First()
                        as EVEJumpTableCodeLine;
                    if (jump < jumptable.JumpAddresses.Count)
                    {
                        str += $"[{opcodeN:D2}] JUMP {jump:X2} ({jump:D3}dec)" + Environment.NewLine;
                        string jumptarget = "";

                        jumptarget += "---------------------------------------------------------------------" + Environment.NewLine;

                        //jumptarget += jumptable.LineByOffset()[jumptable.JumpAddresses[jump]].Parent.ToString();

                        jumptarget += Environment.NewLine + "---------------------------------------------------------------------";

                        //tr += jumptarget.Intend() + Environment.NewLine;
                    }
                }
            }

            str += this.GetBytes().ToHexString();
            return str;
        }
    }
}
