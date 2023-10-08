using GEVLib.GEV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class GevTests
    {
        string TR01 = @"C:\games\wii\0079\0079_jp\DATA\files\event\missionevent\other\TR01.gev";
        string AA02 = @"C:\games\wii\0079\0079_jp\DATA\files\event\missionevent\ace\AA02.gev";

        [Fact]
        public void OFSPaddedToAlignment()
        {
            var expectedBytes = File.ReadAllBytes(AA02);
            var gev = new GEVBinaryRootSegment();
            gev.Parse(expectedBytes);
            var actualBytes = gev.GetBytes().ToArray();

            Assert.Equal(expectedBytes, actualBytes);
        }

        [Fact]
        public void Strings()
        {

            var gev = new GEVBinaryRootSegment();
            gev.Parse(File.ReadAllBytes(TR01));

            var strings = gev.STR.ToString();

            Assert.Equal("EVC_TU_003", gev.STR.Strings.Last());
            Assert.Equal("EVC_TU_003", gev.STR[0x02D0*4]);
            Assert.Equal("EVC_TU_003", gev.OFS[55]);
        }

        [Fact] 
        public void ReadWriteReverse()
        {
            var tr01bytes = File.ReadAllBytes(TR01);
            var eveBytes = tr01bytes.Take(0x1bb0).Skip(0x20 - 4).ToArray();
            var ofsBytes = tr01bytes.Take(0x1c20+4).Skip(0x1bb0).ToArray();
            var strBytes = tr01bytes.Skip(0x1c20 + 4).ToArray();

            var gev = new GEVBinaryRootSegment();
            gev.Parse(tr01bytes);
            var newBytes = gev.GetBytes().ToArray();

            var newEVE = gev.EVE.GetBytes().ToArray();
            var newOFS = gev.OFS.GetBytes().ToArray();
            var newSTR = gev.STR.GetBytes().ToArray();

            Assert.Equal(eveBytes, newEVE);
            Assert.Equal(ofsBytes, newOFS);
            Assert.Equal(strBytes, newSTR);
            Assert.Equal(tr01bytes, newBytes);
        }
    }
}
