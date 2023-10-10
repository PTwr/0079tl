using InMemoryBinaryFile.Helpers;
using System.Text;

namespace GEVLib.EVE
{
    public class EVEJumpTableCodeLine : EVECodeLine
    {
        /// <summary>
        /// Calculate current jumptabble addresses for each code line
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, EVECodeLine> LineByOffset()
        {
            Dictionary<int, EVECodeLine> lineIdByOffset = new Dictionary<int, EVECodeLine>();
            int currentOffset = 0; //offsets are in EVE body space, 0x20 inside the GEV

            foreach (var block in this.Parent.Parent.Children)
            {
                foreach (var line in block.Children)
                {
                    var jumpoffset = $"{line.CodeLineId:X2} {currentOffset:X8} {currentOffset / 4:X4}";
                    lineIdByOffset[currentOffset / 4] = line;
                    currentOffset += line.GetBytes().Count();
                }

                currentOffset += 4; //block terminator (return jump)
            }

            return lineIdByOffset;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Jumptable, pointers to 32bit aligned blocks (opcodes) in EVE body");
            sb.AppendLine("N: Jump Offset => LineId");

            var lineIdByOffset = LineByOffset();
            int n = 0;
            foreach (var addr in JumpAddresses)
            {
                sb.AppendLine(
                    $"JUMP {n:X2} ({n:D3}dec): " +
                    addr.ToString("X4") +
                    " => " +
                    lineIdByOffset[addr].CodeLineId.ToString("X2") +
                    $" ({lineIdByOffset[addr].CodeLineId:D3}dec)" +
                    $" GEV offset: {addr * 4 + 0x20:X8}");

                sb.AppendLine("--------------------------------------------------------------------");
                sb.AppendLine(lineIdByOffset[addr].Parent.ToString().Intend());
                sb.AppendLine("--------------------------------------------------------------------");

                n++;
            }

            return sb.ToString();
        }

        public ushort AddJump(ushort offset)
        {
            JumpAddresses.Add(offset);

            var opcodeA = (0x0013 << 16) + offset;
            var opcodeB = ((JumpAddresses.Count - 1) << 16) + 0xFFFF;

            OpCodes = OpCodes.Select((opcode, n) =>
            {
                if (n % 2 == 1 || opcode == EVEConsts.LineEndDword) return opcode;

                return opcode + 2;
            }).ToList();
            JumpAddresses = JumpAddresses.Select((addr, n) =>
            {
                return (ushort)(addr + 2);
            }).ToList();

            OpCodes.Insert(OpCodes.Count - 1, (uint)opcodeA);
            OpCodes.Insert(OpCodes.Count - 1, (uint)opcodeB);

            LineLength += 2;
            JumpTableEnd += 2;

            return (ushort)(JumpAddresses.Count - 1);
        }

        public EVEJumpTableCodeLine(EVECodeBlock? parent) : base(parent, DefaultHeaderLength + 8)
        {

        }

        protected override int OpCodeCount() => base.OpCodeCount() - 2;
        public List<ushort> JumpAddresses = new List<ushort>();
        protected override void ParseBody(Span<byte> body, Span<byte> everything)
        {
            base.ParseBody(body, everything);

            var jumptableCodes = OpCodes.Take(OpCodes.Count - 1).ToList();

            if (jumptableCodes.Count % 2 != 0) throw new InvalidDataException("Jump table content has to have even number of codes");

            var lastJumpId = -1;
            for (int i = 0; i < jumptableCodes.Count; i += 2)
            {
                var jumpopcode = jumptableCodes[i].GetHighUWORD();
                var jumpoffset = jumptableCodes[i].GetLowUWORD();
                var jumpid = jumptableCodes[i + 1].GetHighUWORD();
                var padding = jumptableCodes[i + 1].GetLowUWORD();

                if (jumpopcode != EVEConsts.JumpCode
                    ||
                    padding != EVEConsts.PaddingCode)
                {
                    throw new InvalidDataException("Not a jump table entry");
                }

                if (lastJumpId + 1 != jumpid)
                {
                    //AR02.gev has jumpids not in order?
                    //throw new InvalidDataException("Non-sequential jump id");
                }
                JumpAddresses.Add(jumpoffset);
                lastJumpId = jumpid;
            }
        }

        protected override void ParseHeader(Span<byte> content, Span<byte> everything)
        {
            base.ParseHeader(content, everything);

            if (content.GetBigEndianUDWORD(DefaultHeaderLength) != EVEConsts.EVECommandStartDword
                ||
                content.GetBigEndianUWORD(DefaultHeaderLength + 4) != EVEConsts.JumpTableEndPointer)
            {
                throw new InvalidDataException("Invalid jump table header");
            }
            JumpTableEnd = content.GetBigEndianUWORD(DefaultHeaderLength + 6);

            if (JumpTableEnd - 1 != LineLength)
            {
                throw new NotSupportedException("Jump table is expected to fully fill code line");
            }
        }
        protected override IEnumerable<byte> HeaderBytes => Concatenate(
            base.HeaderBytes,
            EVEConsts.EVECommandStartDword.GetBigEndianBytes(),
            EVEConsts.JumpTableEndPointer.GetBigEndianBytes(),
            JumpTableEnd.GetBigEndianBytes()
            );

        public ushort JumpTableEnd { get; private set; }

        public static bool Is(Span<byte> body)
        {
            return body.GetBigEndianDWORD(8) == EVEConsts.EVECommandStartDword &&
                body.GetBigEndianWORD(12) == EVEConsts.JumpTableEndPointer;
        }
    }
}
