using GEVLib.EVE;
using GEVLib.Helpers;
using GEVLib.OFS;
using GEVLib.STR;
using InMemoryBinaryFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEVLib.GEV
{
    public class GEVBinaryRootSegment : ParentBinarySegment<IChildBinarySegment<GEVBinaryRootSegment>>
    {
        const int headerLength = 20;
        public GEVBinaryRootSegment(Span<byte> content) : base(content, MagicNumbers.GEVMagicNumber)
        {
            var header = content.Slice(MagicNumbers.GEVMagicNumber.Length, headerLength);
            var body = content.Slice(MagicNumbers.GEVMagicNumber.Length + headerLength);

            UnpackHeader(header);
            UnpackHeader(body);

            children = new List<IChildBinarySegment<GEVBinaryRootSegment>>()
            {
                new EVEBinarySegment(this, content.Slice(MagicNumbers.GEVMagicNumber.Length + headerLength, OFSStart - 4)),
                new OFSBinarySegment(this, content.Slice(OFSStart - 4, STRStart - 4)),
                new STRBinarySegment(this, content.Slice(MagicNumbers.GEVMagicNumber.Length + headerLength, OFSStart - 4)),
            };
        }

        protected override void UnpackHeader(Span<byte> header)
        {
            EVEBlockCount = header.GetBigEndianDWORD(0);
            Alignment = header.GetBigEndianDWORD(4);
            OFSStart = header.GetBigEndianDWORD(8);
            OFSStart = header.GetBigEndianDWORD(12);
            STRStart = header.GetBigEndianDWORD(16);
        }

        protected override void UnpackBody(Span<byte> body)
        {
            throw new NotImplementedException();
        }

        public int EVEBlockCount { get; private set; }
        public int Alignment { get; private set; }
        public int OFSWordCount { get; private set; }
        public int OFSStart { get; private set; }
        public int STRStart { get; private set; }

        public EVEBinarySegment EVE => (EVEBinarySegment)this.children[0];
        public OFSBinarySegment OFS => (OFSBinarySegment)this.children[1];
        public STRBinarySegment STR => (STRBinarySegment)this.children[2];
    }
}
