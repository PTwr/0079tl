using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InMemoryBinaryFile.Helpers;

namespace GEVLib.EVE
{
    public class EVESoundPlayback : EVECodeLine
    {
        public EVESoundPlayback(EVECodeBlock? parent, int headerLength = 8) : base(parent, headerLength)
        {
        }

        public static bool Is(Span<byte> body)
        {
            return body.GetBigEndianDWORD(8) == EVEConsts.VoiceBlock.Opcode1 &&
                body.GetBigEndianWORD(12) == EVEConsts.VoiceBlock.Opcode2;
        }

        public string VoiceFile() => this.Parent.Parent.Parent
            .OFS[this.OpCodes[1].GetLowUWORD()];

        public override string ToString()
        {
            var str = $"One-time voice playback?" + Environment.NewLine;
            str += base.ToString();
            return str;
        }
    }
}
