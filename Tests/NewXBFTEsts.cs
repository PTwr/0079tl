using InMemoryBinaryFile.New.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using XBFLib;
using XBFLib.New;

namespace Tests
{
    public class NewXBFTEsts
    {
        string testfile = @"C:\games\wii\0079\0079_unpacked\DATA\files\_2d\Title\Title_text.arc\arc\MAIN_MENU.arc\arc\BlockText.xbf";
        [Fact]
        public void HeaderTest()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var bytes = File.ReadAllBytes(testfile);

            var parsed = new XbfRootSegment(true);
            parsed.Parse(bytes.AsSpan());

            XbfFile newXBF = new XbfFile(Encoding.UTF8);
            newXBF = Deserializer.Deserialize<XbfFile>(bytes.AsSpan(), newXBF);

            Assert.Equal(parsed.TreePosition, newXBF.TreePosition);
            Assert.Equal(parsed.TreeLength, newXBF.TreeCount);

            Assert.Equal(parsed.NodeDictPosition, newXBF.NodeDictPosition);
            Assert.Equal(parsed.NodeDictLength, newXBF.NodeDictCount);

            Assert.Equal(parsed.AttributeDictPosition, newXBF.AttributeDictPosition);
            Assert.Equal(parsed.AttributeDictLength, newXBF.AttributeDictCount);

            Assert.Equal(parsed.StringDictPosition, newXBF.StringDictPosition);
            Assert.Equal(parsed.StringDictLength, newXBF.StringDictCount);

            Assert.Equal(parsed.StringDict.Values, newXBF.StringDict.Values);
            Assert.Equal(parsed.AttributeDict.Values, newXBF.AttributeDict.Values);
            Assert.Equal(parsed.NodeDict.Values, newXBF.NodeDict.Values);
        }
    }
}
