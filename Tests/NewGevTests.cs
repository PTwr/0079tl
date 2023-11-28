using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GEVLib.GEV;
using GEVLib.New;
using InMemoryBinaryFile.Helpers;
using InMemoryBinaryFile.New.Serialization;

namespace Tests
{
    public class NewGevTests
    {
        [Fact]
        public void Read()
        {


            var tr01gev = @"C:\games\wii\0079\0079_jp\DATA\files\event\missionevent\other\TR01.gev";
            var bytes = File.ReadAllBytes(tr01gev).AsSpan();

            var gevOld = new GEVBinaryRootSegment();
            gevOld.Parse(bytes);

            var gevNew = Deserializer.Deserialize<GEVFile>(bytes);

            Assert.Equal(gevOld.EVEBlockCount, gevNew.EVEBlockCount);
            Assert.Equal(gevOld.Alignment, gevNew.Alignment);
            Assert.Equal(gevOld.OFSWordLength, gevNew.OFSDataCount);
            Assert.Equal(gevOld.OFSStart, gevNew.OFSDataOffset);
            Assert.Equal(gevOld.STRStart, gevNew.STRDataOffset);

            Assert.Equal(gevOld.STR.IndexedStrings.Values.ToList(), gevNew.OFSSegment.OFSEntries.Select(i => i.ToString()).ToList());


            var bbb = Serializer.Serialize(gevNew);

            Assert.Equal(bytes.ToArray().ToList(), bbb);


            var sss = gevNew.EVESegment.ToString();
        }


    }
}
