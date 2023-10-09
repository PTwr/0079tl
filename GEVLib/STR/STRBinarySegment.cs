using GEVLib.EVE;
using GEVLib.GEV;
using InMemoryBinaryFile;
using InMemoryBinaryFile.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEVLib.STR
{
    public class STRBinarySegment : StringDictSegment<GEVBinaryRootSegment>
    {
        public const string magicNumber = "$STR";
        const int headerLength = 5 * 4;
        public STRBinarySegment(GEVBinaryRootSegment Parent, Dictionary<string, string>? translations = null) : base(Parent, EncodingHelper.Shift_JIS, magicNumber.ToASCIIBytes(), alignment: 4, translations: translations)
        {
        }
    }
}
