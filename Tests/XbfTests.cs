using InMemoryBinaryFile.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XBFLib;

namespace Tests
{
    public class XbfTests
    {
        //string testfile = @"C:\DEV\0079tl\Anim_BRF_PhotoA_00.xbf.bin";
        //string testfile = @"E:\0079_jp\DATA\files\parameter\result_param_format.xbf";
        //string testfile = @"C:\DEV\0079tl\FlagCtrl.xbf.bin";
        string testfile = @"C:\DEV\0079tl\combat_param.xbf.bin";

        [Fact]
        public void EncodingTest()
        {
            //d_cannon_<0xef><0xbd><0x92> gets malformed upon save
            var expected = File.ReadAllBytes(@"C:\DEV\0079tl\combat_param.xbf.bin");

            var csharpstr = "d_cannon\xEF\xBD\x92"; //probably Windows 1250

            var bytes = expected.AsSpan().FindNullTerminator(0x01_C8_D2);

            var asciiStr = bytes.ToAsciiString();
            var shiftJisStr = bytes.ToShiftJisString();
            var w1250 = bytes.ToW1250String();

            var b1 = bytes.ToArray();

            string encod = "";

            foreach(var ei in System.Text.Encoding.GetEncodings())
            {
                var s = ei.GetEncoding().GetString(bytes);
                var b2 = ei.GetEncoding().GetBytes(s);
                var b3 = ei.GetEncoding().GetBytes(csharpstr);

                if (s == csharpstr)
                {
                    Debugger.Break();
                }

                if (Enumerable.SequenceEqual(b1, b2))
                {
                    encod += $"{ei.Name} {ei.CodePage}{Environment.NewLine}";
                }

                if (Enumerable.SequenceEqual(b1, b3))
                {
                    Debugger.Break();
                }

            }

            var b4 = Encoding.GetEncoding(1250).GetBytes(Encoding.GetEncoding(1250).GetString(bytes));
            var sss = Encoding.GetEncoding(1250).GetString(bytes);
            var b5 = Encoding.GetEncoding(1250).GetBytes(sss);

            var s6 = EncodingHelper.Windows1250.GetString(bytes);
            var b6 = EncodingHelper.Windows1250.GetBytes(s6);

            //can't represent that legacy crap in crappy c# unicode
            Assert.NotEqual(csharpstr, asciiStr);
            Assert.NotEqual(csharpstr, shiftJisStr);
            Assert.NotEqual(csharpstr, w1250);
            Assert.Equal(b1, b4);
            Assert.Equal(b1, b5);
            Assert.Equal(b1, b6);
        }

        [Fact]
        public void ReadWriteLoop()
        {
            var expected = File.ReadAllBytes(testfile);

            var parsed = new XbfRootSegment();
            parsed.Parse(expected.AsSpan());

            var dumpedBytes = parsed.GetBytes().ToArray();

            //int n = 0;
            //foreach (var c in parsed.Children)
            //{
            //    File.WriteAllBytes(@$"C:\DEV\0079tl\Anim_BRF_PhotoA_00.xbf_{n}.bin", c.GetBytes().ToArray());
            //    n++;
            //}
            //File.WriteAllBytes(@$"C:\DEV\0079tl\Anim_BRF_PhotoA_00.xbf_recreated.bin", dumpedBytes);

            Assert.Equal(expected.Length, dumpedBytes.Length);
            Assert.Equal(expected, dumpedBytes);
        }

        [Fact]
        public void RecreateFromXml()
        {
            var expected = File.ReadAllBytes(testfile);

            var parsed = new XbfRootSegment();
            parsed.Parse(expected.AsSpan());

            var dumpedBytes = parsed.GetBytes().ToArray();

            //check if just dumping works
            Assert.Equal(expected.Length, dumpedBytes.Length);
            Assert.Equal(expected, dumpedBytes);

            var recreated = new XbfRootSegment(parsed.NodeTree.XmlDocument);

            dumpedBytes = recreated.GetBytes().ToArray();
            //File.WriteAllBytes(@$"C:\DEV\0079tl\FlagCtrl.xbf.recreated.bin", dumpedBytes);

            Assert.Equal(expected.Length, dumpedBytes.Length);
            Assert.Equal(expected, dumpedBytes);
        }

        [Fact]
        public void TestAllXbfFiles()
        {
            var files = new System.IO.DirectoryInfo(@"E:\0079_jp")
                .GetFiles("*.xbf", SearchOption.AllDirectories);

            int c = 0;
            foreach (var file in files)
            {
                var expected = File.ReadAllBytes(file.FullName);

                var parsed = new XbfRootSegment();
                parsed.Parse(expected.AsSpan());

                var dumpedBytes = parsed.GetBytes().ToArray();

                var xml = parsed.NodeTree.ToString();

                //check if just dumping works
                Assert.Equal(expected.Length, dumpedBytes.Length);
                Assert.Equal(expected, dumpedBytes);

                var recreated = new XbfRootSegment(parsed.NodeTree.XmlDocument);

                dumpedBytes = recreated.GetBytes().ToArray();

                //check if recreating from xml returns identical bytes
                Assert.Equal(expected.Length, dumpedBytes.Length);
                Assert.Equal(expected, dumpedBytes);

                c++;
            }
        }
    }
}
