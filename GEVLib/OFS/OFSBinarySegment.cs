using GEVLib.EVE;
using GEVLib.GEV;
using InMemoryBinaryFile;
using InMemoryBinaryFile.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEVLib.OFS
{
    public class OFSBinarySegment : _BaseBinarySegment<GEVBinaryRootSegment>
    {
        public const string magicNumber = "$OFS";
        public OFSBinarySegment(GEVBinaryRootSegment Parent) : base(Parent, magicNumber.ToASCIIBytes())
        {
        }

        List<ushort> stringIndexes = new List<ushort>();
        protected override void ParseBody(Span<byte> body, Span<byte> everything)
        {
            stringIndexes = new List<ushort>();
            for (int i = 0; i < Parent.OFSWordLength; i++)
            {
                stringIndexes.Add(body.GetBigEndianUWORD(i * 2));
            }

            ValidateOFSIsSequential();
        }
        protected override IEnumerable<byte> BodyBytes => stringIndexes.SelectMany(i=>i.GetBigEndianBytes());

        protected override void ParseHeader(Span<byte> header, Span<byte> everything)
        {
            //nothing to do here
        }

        protected void ValidateOFSIsSequential()
        {
            if (!stringIndexes.SequenceEqual(stringIndexes.OrderBy(i => i)))
            {
                throw new NotImplementedException("Non-incremental OFS indexes are not supported");
            }
        }

        public string this[int i]
        {
            get { return this.Parent.STR[stringIndexes[i] * 4]; }
        }

        public void UpdateIndexes()
        {
            stringIndexes.Clear();
            foreach(var str in Parent.STR.IndexedStrings)
            {
                stringIndexes.Add((ushort)(str.Key / 4));
            }
        }
    }
}
