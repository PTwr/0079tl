using GEVLib.EVE;
using GEVLib.GEV;
using InMemoryBinaryFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEVLib.STR
{
    public class STRBinarySegment : HierarchicalBinarySegment<IChildBinarySegment<STRBinarySegment>, GEVBinaryRootSegment>
    {
        public STRBinarySegment(GEVBinaryRootSegment Parent, Span<byte> content) : base(Parent, content, MagicNumbers.STRMagicNumber)
        {
        }

        protected override void UnpackBody(Span<byte> body)
        {
            throw new NotImplementedException();
        }

        protected override void UnpackHeader(Span<byte> header)
        {
            throw new NotImplementedException();
        }
    }
}
