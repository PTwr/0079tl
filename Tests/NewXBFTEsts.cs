using InMemoryBinaryFile.New;
using InMemoryBinaryFile.New.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using U8.New;
using XBFLib;
using XBFLib.New;

namespace Tests
{
    public class NewXBFTEsts
    {
        string u8testfile = @"C:\games\wii\0079\0079_jp\DATA\files\_2d\Briefing\BR_AA01.arc";
        string u8testfiledirectories = @"C:\games\wii\0079\0079_jp\DATA\files\hbm\homeBtn.arc";
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

            Assert.Equal(parsed.TreePosition, newXBF.TreeOffset);
            Assert.Equal(parsed.TreeLength, newXBF.TreeCount);

            Assert.Equal(parsed.NodeDictPosition, newXBF.NodeDictOffset);
            Assert.Equal(parsed.NodeDictLength, newXBF.NodeDictCount);

            Assert.Equal(parsed.AttributeDictPosition, newXBF.AttributeDictOffset);
            Assert.Equal(parsed.AttributeDictLength, newXBF.AttributeDictCount);

            Assert.Equal(parsed.StringDictPosition, newXBF.StringDictOffset);
            Assert.Equal(parsed.StringDictLength, newXBF.StringDictCount);

            Assert.Equal(parsed.StringDict.Values, newXBF.StringDict.Values);
            Assert.Equal(parsed.AttributeDict.Values, newXBF.AttributeDict.Values);
            Assert.Equal(parsed.NodeDict.Values, newXBF.NodeDict.Values);

            var oldxml = parsed.NodeTree.ToString();
            var newxml = newXBF.ToString();

            Assert.Equal(oldxml, newxml);
        }

        [Fact]
        public void TestU8()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var bytes = File.ReadAllBytes(u8testfile);

            var newU8 = Deserializer.Deserialize<U8File>(bytes.AsSpan());
            var bb = newU8.NodeList[2].BinaryData.AsSpan();

            var xx = Deserializer.Deserialize<XbfFile>(bb);

            var xml = xx.GetXmlDocument();
            var str = xx.GetXmlString();
        }

        [Fact]
        public void TestU8Dirs()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var bytes = File.ReadAllBytes(u8testfiledirectories);

            var newU8 = Deserializer.Deserialize<U8File>(bytes.AsSpan());
        }
    }
}
