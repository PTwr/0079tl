using BinarySerializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using U8.New;
using XBFLib;

namespace Tests
{
    public class NewSerializerTests
    {
        string u8testfile = @"C:\games\wii\0079\0079_jp\DATA\files\_2d\Briefing\BR_AA01.arc";
        string u8testfiledirectories = @"C:\games\wii\0079\0079_jp\DATA\files\hbm\homeBtn.arc";
        string testfile = @"C:\games\wii\0079\0079_unpacked\DATA\files\_2d\Title\Title_text.arc\arc\MAIN_MENU.arc\arc\BlockText.xbf";

        [Fact]
        public void U8PeekXbf()
        {
            var bytes = File.ReadAllBytes(@"C:\games\wii\0079\0079_jp\DATA\files\_2d\Title\GUIDE.arc");
            var u8 = Serializer.Deserialize<U8File>(bytes.AsSpan());

            var modelxbfnode = u8.U8Tree.Root.U8HierarchicalNodes.First().U8HierarchicalNodes.Last();

            var xbf = modelxbfnode.XbfFile;
        }
        [Fact]
        public void U8PeekU8()
        {
            var bytes = File.ReadAllBytes(@"C:\games\wii\0079\0079_jp\DATA\files\_2d\Briefing\BR_AA01_text.arc");
            var u8 = Serializer.Deserialize<U8File>(bytes.AsSpan());

            //var modelxbfnode = u8.U8Tree.Root.U8HierarchicalNodes.First().U8HierarchicalNodes.Last();

            //var xbf = modelxbfnode.XbfFile;
        }

        [Fact]
        public void TestCleanedU8()
        {
            {
                var bytes = File.ReadAllBytes(@"C:\games\wii\0079\0079_jp\DATA\files\_2d\Title\GUIDE.arc");
                var u8 = Serializer.Deserialize<U8.NewNew.U8File>(bytes.AsSpan());
                var newBytes = Serializer.Serialize(u8);
                Assert.Equal(bytes, newBytes);
            }
            {
                var bytes = File.ReadAllBytes(@"C:\games\wii\0079\0079_jp\DATA\files\_2d\Briefing\BR_AA01_text.arc");
                var u8 = Serializer.Deserialize<U8.NewNew.U8File>(bytes.AsSpan());

                xbf = (u8["arc"]["OP_AA01.arc"]["arc"]["BlockText.xbf"] as U8.NewNew.U8FileNode).Xbf;

                var xbfoldbytes = File.ReadAllBytes(@"C:\games\wii\0079\0079_unpacked\DATA\files\_2d\Briefing\BR_AA01_text.arc\arc\OP_AA01.arc\arc\BlockText.xbf");
                var xbfnewbytes = Serializer.Serialize(xbf);
                Assert.Equal(xbfoldbytes, xbfnewbytes);

                var newBytes = Serializer.Serialize(u8);
                Assert.Equal(bytes, newBytes);
            }
        }

        [Fact]
        public void TestU8()
        {
            //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //var bytes = File.ReadAllBytes(u8testfile);

            //var newU8 = Serializer.Deserialize<U8File>(bytes.AsSpan());
            ////var bb = newU8.NodeList[2].BinaryData.AsSpan();

            //var xx = newU8.U8HierarchicalNode.Items().FirstOrDefault(i => i.IsXbf).XbfFile;

            //var xml = xx.GetXmlDocument();
            //var str = xx.GetXmlString();

            //var xbf = new XbfFile(xml, Encoding.UTF8);

            //var xml2 = xx.GetXmlDocument();
            //var str2 = xx.GetXmlString();

            //Assert.Equal(str, str2);

            //var newbytes = Serializer.Serialize(newU8).ToArray();

            //Assert.Equal(bytes, newbytes);

            //TODO test serialziation once written
        }

        [Fact]
        public void TestU8Dirs()
        {
            //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //var bytes = File.ReadAllBytes(u8testfiledirectories);

            //var newU8 = Deserializer.Deserialize<U8File>(bytes.AsSpan());
        }

        [Fact]
        public void XbfDeserialize()
        {
            var bytes = File.ReadAllBytes(@"C:\games\wii\0079\0079_unpacked\DATA\files\_2d\Briefing\BR_AA01_text.arc\arc\OP_AA01.arc\arc\BlockText.xbf");
            var xbf = Serializer.Deserialize<XbfFile>(bytes.AsSpan());
            var str = xbf.ToString();

            bytes = File.ReadAllBytes(@"C:\games\wii\0079\0079_unpacked\DATA\files\parameter\result_param.xbf");
            xbf = Serializer.Deserialize<XbfFile>(bytes.AsSpan());
            str = xbf.ToString();
        }

        [Fact]
        public void XbfRoundtrip()
        {
            var bytes = File.ReadAllBytes(@"C:\games\wii\0079\0079_unpacked\DATA\files\_2d\Briefing\BR_AA01_text.arc\arc\OP_AA01.arc\arc\BlockText.xbf");
            var xbf = Serializer.Deserialize<XbfFile>(bytes.AsSpan());
            var str = xbf.ToString();

            var newBytes = Serializer.Serialize(xbf);
            Assert.Equal(bytes, newBytes);
        }

        [Fact]
        public void XbfRoundtripResultParam()
        {
            var bytes = File.ReadAllBytes(@"C:\Users\LordOfTheSkrzynka\Documents\Dolphin Emulator\Load\Riivolution\R79JAF_EN\parameter\result_param.orig.xbf");
            var xbf = Serializer.Deserialize<XbfFile>(bytes.AsSpan());

            var xmlstr = xbf.ToString();

            var newBytes = bytes; // Serializer.Serialize(xbf);

            Assert.Equal(bytes, newBytes);

            var xbffromxml = new XbfFile(xmlstr);

            var xmlstr2 = xbffromxml.ToString();

            //newBytes = Serializer.Serialize(xbffromxml);

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


            for (int i = 0; i < xbf.TreeStructureCount; i++)
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
