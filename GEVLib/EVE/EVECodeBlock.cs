using InMemoryBinaryFile;
using InMemoryBinaryFile.Helpers;
using System.Text;

namespace GEVLib.EVE
{
    public class EVECodeBlock : ParentBinarySegment<EVEBinarySegment, EVECodeLine>
    {
        public EVECodeBlock(EVEBinarySegment? parent) : base(parent, headerLength: 0)
        {
        }

        protected override void ParseBody(Span<byte> body, Span<byte> everything)
        {
            int offset = 0;

            children.Clear();

            EVECodeLine line;
            do
            {
                if (EVEJumpTableCodeLine.Is(body.Slice(offset)))
                    line = new EVEJumpTableCodeLine(this);
                else if (EVESoundPlayback.Is(body.Slice(offset)))
                    line = new EVESoundPlayback(this);
                else
                    line = new EVECodeLine(this);

                line.Parse(body.Slice(offset));

                offset += line.GetBytes().Count();
                //body = body.Slice(offset);

                children.Add(line);

                if (offset+0x8c0 ==  0xa30)
                {

                }
            }
            while (!(body.GetBigEndianUDWORD(offset) == EVEConsts.ReturnDword
                || 
                body.GetBigEndianUDWORD(offset) == EVEConsts.FinalTerminatorDword));


            children.Last().IsLastLine = true;
        }
        protected override List<EVECodeLine> children { get; } = new List<EVECodeLine>();

        protected override IEnumerable<byte> BodyBytes => base.BodyBytes.Concat(EVEConsts.ReturnDword.GetBigEndianBytes());

        protected override void ParseHeader(Span<byte> header, Span<byte> everything)
        {
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var line in children)
            {
                sb.AppendLine(line.ToString().Intend());
                sb.AppendLine("------------------------------------------------------------------------");
            }

            return sb.ToString();
        }
    }
}
