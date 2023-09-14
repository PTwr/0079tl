using GEVLib.EVE;
using GEVLib.GEV;
using InMemoryBinaryFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEVLib.OFS
{
    public class OFSBinarySegment : HierarchicalBinarySegment<IChildBinarySegment<OFSBinarySegment>, GEVBinaryRootSegment>
    {
        public OFSBinarySegment(GEVBinaryRootSegment Parent, Span<byte> content) : base(Parent, content, MagicNumbers.OFSMagicNumber)
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
