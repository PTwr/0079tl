using GEVLib.GEV;
using InMemoryBinaryFile;
using InMemoryBinaryFile.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace GEVLib.EVE
{
    public class EVEBinarySegment : ParentBinarySegment<GEVBinaryRootSegment, EVECodeBlock>
    {
        public const string magicNumber = "$EVE";
        const int headerLength = 0;
        public EVEBinarySegment(GEVBinaryRootSegment Parent) : base(Parent, magicNumber.ToASCIIBytes(), headerLength)
        {
        }

        EVEJumpTableCodeLine JumpTable => children.First().Children.First() as EVEJumpTableCodeLine;

        public void InsertTextbox(ushort jumpId, ushort jumpoutlineid, string text)
        {
            var jumps = this.JumpTable.LineByOffset();

            Parent.STR.AddString(text);
            Parent.OFS.UpdateIndexes();
            var ofsId = ((ushort)(Parent.OFS.StringIndexes.Count - 1)).GetBigEndianBytes(); //new string is always last
            var lineId = this.children.Last().Children.Last().CodeLineId;
            //point at last dword, 0x0006FFF, thats where new block will start
            var newJumppointOffset = this.BodyBytes.Count() + 4;

            var bytes = new byte[]
            {
                0x00, 0x01, (lineId+1).GetBigEndianBytes()[2], (lineId+1).GetBigEndianBytes()[3],
                0x00, 0x06, 0x00, 0x02,
                0x00, 0x03, 0x00, 0x00,
                0x00, 0xC1, ofsId[0], ofsId[1], //text reference
                0x00, 0xC0, 0x00, 0x01,
                0x00, 0x04, 0x00, 0x00,

                0x00, 0x01, (lineId+2).GetBigEndianBytes()[2], (lineId+2).GetBigEndianBytes()[3],
                0x00, 0x07, 0x00, 0x03, 
                0x00, 0x29, 0x00, 0x00, 
                0x00, 0x03, 0x00, 0x00,
                0x00, 0xC2, 0xFF, 0xFF,
                0x00, 0x0C, 0x00, 0x01,
                0x00, 0x04, 0x00, 0x00,

                0x00, 0x01, (lineId+3).GetBigEndianBytes()[2], (lineId+3).GetBigEndianBytes()[3],
                0x00, 0x07, 0x00, 0x03, 
                0x00, 0xC3, 0xFF, 0xFF,
                0x00, 0x03, 0x00, 0x00,
                //execute overriden jump
                0x00, 0x11, jumpId.GetBigEndianBytes()[0], jumpId.GetBigEndianBytes()[1],
                0x00, 0x0C, 0x00, 0x01,
                0x00, 0x04, 0x00, 0x00,

                EVEConsts.ReturnDword.GetBigEndianBytes()[0], EVEConsts.ReturnDword.GetBigEndianBytes()[1],
                EVEConsts.ReturnDword.GetBigEndianBytes()[2], EVEConsts.ReturnDword.GetBigEndianBytes()[3]
            };

            var block = new EVECodeBlock(this);
            block.Parse(bytes.AsSpan());
            this.children.Add(block);

            var newJump = JumpTable.AddJump((ushort)(newJumppointOffset / 4));

            //override old jump

            var modline = this.children.SelectMany(i => i.Children).FirstOrDefault(i => i.CodeLineId == jumpoutlineid);

            modline.ReplaceOpcode((uint)(0x00110000 + jumpId), (uint)(0x00110000 + newJump));
        }

        protected override void ParseBody(Span<byte> body, Span<byte> everything)
        {
            var lastDword = body.GetBigEndianDWORD(body.Length - 4);
            if (lastDword != EVEConsts.FinalTerminatorDword)
            {
                throw new NotSupportedException($"EVE last DWORD is expected to be {EVEConsts.FinalTerminatorDword:X8}, not {lastDword:X8}");
            }

            int offset = 0;
            while (offset < body.Length && body.GetBigEndianDWORD(offset) != EVEConsts.FinalTerminatorDword)
            {
                var hex = (offset + 0x20).ToString("X8");

                EVECodeBlock block = new EVECodeBlock(this);
                block.Parse(body.Slice(offset));
                children.Add(block);

                offset += block.GetBytes().Count();
            }
        }
        protected override List<EVECodeBlock> children { get; } = new List<EVECodeBlock>();

        protected override void ParseHeader(Span<byte> header, Span<byte> everything)
        {
            //throw new NotImplementedException();
        }

        protected override IEnumerable<byte> BodyBytes => children
            .SelectMany(i => i.GetBytes()).
            ConcatIf(EVEConsts.FinalTerminatorDword.GetBigEndianBytes(), true);

        public override string ToString()
        {
            var sb = new StringBuilder();

            int blockN = 0;
            foreach (var block in children)
            {
                sb.AppendLine($"---------------------------------{blockN:D3}------------------------------------");
                sb.AppendLine(block.ToString().Intend());
                sb.AppendLine("------------------------------------------------------------------------");
                blockN++;
            }

            return sb.ToString();
        }
    }
}
