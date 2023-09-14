using GEVLib.GEV;
using InMemoryBinaryFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEVLib.EVE
{
    public class EVEBinarySegment : HierarchicalBinarySegment<IChildBinarySegment<EVEBinarySegment>, GEVBinaryRootSegment>
    {
        public EVEBinarySegment(GEVBinaryRootSegment Parent, Span<byte> content) : base(Parent, content, MagicNumbers.EVEMagicNumber)
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
