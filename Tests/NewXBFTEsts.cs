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

            Assert.Equal(parsed.TreePosition, newXBF.TreeStructureOffset);
            Assert.Equal(parsed.TreeLength, newXBF.TreeStructureCount);

            Assert.Equal(parsed.NodeDictPosition, newXBF.TagListOffset);
            Assert.Equal(parsed.NodeDictLength, newXBF.TagListCount);

            Assert.Equal(parsed.AttributeDictPosition, newXBF.AttributeListOffset);
            Assert.Equal(parsed.AttributeDictLength, newXBF.AttributeListCount);

            Assert.Equal(parsed.StringDictPosition, newXBF.ValueListOffset);
            Assert.Equal(parsed.StringDictLength, newXBF.ValueListCount);

            Assert.Equal(parsed.StringDict.Values, newXBF.ValueList);
            Assert.Equal(parsed.AttributeDict.Values, newXBF.AttributeList);
            Assert.Equal(parsed.NodeDict.Values, newXBF.TagList);

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
            //var bb = newU8.NodeList[2].BinaryData.AsSpan();

            var xx = newU8.U8HierarchicalNode.Items().FirstOrDefault(i => i.IsXbf).XbfFile;

            var xml = xx.GetXmlDocument();
            var str = xx.GetXmlString();

            var xbf = new XbfFile(xml, Encoding.UTF8);

            var xml2 = xx.GetXmlDocument();
            var str2 = xx.GetXmlString();

            Assert.Equal(str, str2);

            //TODO test serialziation once written
        }

        [Fact]
        public void TestU8Dirs()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var bytes = File.ReadAllBytes(u8testfiledirectories);

            var newU8 = Deserializer.Deserialize<U8File>(bytes.AsSpan());
        }

        [Fact]
        public void XbfRoundtrip()
        {
            var bytes = File.ReadAllBytes(@"C:\games\wii\0079\0079_unpacked\DATA\files\_2d\Briefing\BR_AA01_text.arc\arc\OP_AA01.arc\arc\StringGroup.xbf");
            var xbf = Deserializer.Deserialize<XbfFile>(bytes.AsSpan());

            var xmlstr = xbf.ToString();

            var newBytes = Serializer.Serialize(xbf);

            Assert.Equal(bytes, newBytes);

            var xbffromxml = new XbfFile(xmlstr);

            var xmlstr2 = xbffromxml.ToString();

            newBytes = Serializer.Serialize(xbffromxml);

            Assert.Equal(xbf.TreeStructureOffset, xbffromxml.TreeStructureOffset);
            Assert.Equal(xbf.TreeStructureOffset, xbffromxml.TreeStructureOffset);
            Assert.Equal(xbf.TreeStructureOffset, xbffromxml.TreeStructureOffset);

            Assert.Equal(xbf.TreeStructureCount, xbffromxml.TreeStructureCount);
            Assert.Equal(xbf.TagListCount, xbffromxml.TagListCount);
            Assert.Equal(xbf.AttributeListCount, xbffromxml.AttributeListCount);
            Assert.Equal(xbf.ValueListCount, xbffromxml.ValueListCount);

            Assert.Equal(xbf.TreeStructureOffset, xbffromxml.TreeStructureOffset);
            Assert.Equal(xbf.TagListOffset, xbffromxml.TagListOffset);
            Assert.Equal(xbf.AttributeListOffset, xbffromxml.AttributeListOffset);
            Assert.Equal(xbf.ValueListOffset, xbffromxml.ValueListOffset);


            for(int i=0;i<xbf.TreeStructureCount;i++)
            {
                Assert.Equal(xbf.TreeStructure[i].ToString(), xbffromxml.TreeStructure[i].ToString());
                Assert.Equal(xbf.TreeStructure[i].NameOrAttributeId, xbffromxml.TreeStructure[i].NameOrAttributeId);
                Assert.Equal(xbf.TreeStructure[i].ValueId, xbffromxml.TreeStructure[i].ValueId);
            }

            Assert.Equal(bytes, newBytes);

            Assert.Equal(xmlstr, xmlstr2);
        }
    }
}
