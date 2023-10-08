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

        public List<ushort> StringIndexes { get; private set; } = new List<ushort>();
        protected override void ParseBody(Span<byte> body, Span<byte> everything)
        {
            StringIndexes = new List<ushort>();
            for (int i = 0; i < Parent.OFSWordLength; i++)
            {
                StringIndexes.Add(body.GetBigEndianUWORD(i * 2));
            }

            ValidateOFSIsSequential();
        }
        protected override IEnumerable<byte> BodyBytes => StringIndexes.SelectMany(i=>i.GetBigEndianBytes()).PadToAlignment(Parent.Alignment/8);

        protected override void ParseHeader(Span<byte> header, Span<byte> everything)
        {
            //nothing to do here
        }

        protected void ValidateOFSIsSequential()
        {
            if (!StringIndexes.SequenceEqual(StringIndexes.OrderBy(i => i)))
            {
                throw new NotImplementedException("Non-incremental OFS indexes are not supported");
            }
        }

        public string this[int i]
        {
            get { return this.Parent.STR[StringIndexes[i] * 4]; }
        }

        public void UpdateIndexes()
        {
            StringIndexes.Clear();
            foreach(var str in Parent.STR.IndexedStrings)
            {
                StringIndexes.Add((ushort)(str.Key / 4));
            }
        }
    }
}
