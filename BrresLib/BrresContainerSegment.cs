using InMemoryBinaryFile;
using InMemoryBinaryFile.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrresLib
{
    public class BrresContainerSegment : ParentBinarySegment<IBinarySegment, _BaseBinarySegment<BrresContainerSegment>>
    {
        public const string magicNumber = "bres";
        const int headerLength = 2 + 2 + 4 + 2 + 2;
        public BrresContainerSegment() : base(null, magicNumber.ToASCIIBytes(), headerLength)
        {
        }

        protected override void ParseBody(Span<byte> body, Span<byte> everything)
        {
            //var blah = body.Slice(3 * 16); //header shit
            //throw new NotImplementedException();

            //for(int n=16*3;n<everything.Length;n+=4)
            //{
            //    var x = everything.GetBigEndianDWORD(n);
            //    if (x==0x00163f9c)
            //    {
            //        var strOffsethex = x.ToString("X");
            //        var addr = n.ToString("X");
            //    }
            //}

            //for (int pos= 16*3; pos < everything.Length; pos+=16)
            //{
            //    var entry = everything.Slice(pos, 16);
            //    var stringOffset = entry.GetBigEndianDWORD(8);
            //    stringOffset += 20;
            //    var strOffsethex = stringOffset.ToString("X");
            //    var stringLength = everything.GetBigEndianDWORD(stringOffset);

            //    var stringBytes = everything.Slice(stringOffset+4, stringLength);

            //    var name = stringBytes.ToUTF8String();
            //}
            BrresRootSegment root = new BrresRootSegment(this);

            //TODO check if rootOffset == header.length
            root.Parse(everything.Slice(rootOffset));
        }

        protected override void ParseHeader(Span<byte> header, Span<byte> everything)
        {
            byteOrderMark = header.GetBigEndianUWORD(0);
            padding = header.GetBigEndianUWORD(2);
            length = header.GetBigEndianUWORD(4); //address to string table?
            rootOffset = header.GetBigEndianUWORD(8);
            sectionCount = header.GetBigEndianUWORD(10);
        }
        //BrresRootNodeSegment rootNode = null;
        ushort byteOrderMark;
        ushort padding;
        uint length;
        public ushort rootOffset;
        ushort sectionCount;
    }

    public class BrresRootSegment : ParentBinarySegment<BrresContainerSegment, _BaseBinarySegment<BrresRootSegment>>
    {
        public const string magicNumber = "root";
        const int headerLength = 4;
        public BrresRootSegment(BrresContainerSegment parent) : base(parent, magicNumber.ToASCIIBytes(), headerLength)
        {
        }

        protected override void ParseBody(Span<byte> body, Span<byte> everything)
        {
            //throw new NotImplementedException();

            List<string> names = new List<string>();
            int sectionCounter = 0;
            for(int pos = 0; pos < body.Length;)
            {
                sectionCounter++;

                var length = body.GetBigEndianDWORD(pos);
                var children = body.GetBigEndianDWORD(pos + 4);

                var poshex = (pos + 24).ToString("X8");
                var lengthhex = (length).ToString("X8");
                var childrenhex = (children).ToString("X8");

                var a = body.GetBigEndianDWORD(pos+8).ToString("X8"); //FFFF0000
                var c = body.GetBigEndianDWORD(pos+12).ToString("X8"); //number
                var d = body.GetBigEndianDWORD(pos+16).ToString("X8"); //00000000
                var e = body.GetBigEndianDWORD(pos+20).ToString("X8"); //00000000

                for (int childpos = pos+24, n = 0;n<children;n++,childpos+=16)
                {
                    var id = body.GetBigEndianWORD(childpos).ToString("X4");
                    var unk = body.GetBigEndianWORD(childpos+2).ToString("X4");
                    var lid = body.GetBigEndianWORD(childpos+4).ToString("X4");
                    var rid = body.GetBigEndianWORD(childpos+6).ToString("X4");
                    var nameoffset = body.GetBigEndianDWORD(childpos+8).ToString("X8");
                    var dataoffset = body.GetBigEndianDWORD(childpos+12).ToString("X8");

                    var stringOffset = body.GetBigEndianDWORD(childpos + 8);
                    var strOffsethex = stringOffset.ToString("X");

                    stringOffset += pos;
                    var strOffsethex2 = stringOffset.ToString("X");

                    var stringLength = body.GetBigEndianDWORD(stringOffset-4);

                    var stringBytes = body.Slice(stringOffset, stringLength);

                    var str = stringBytes.ToUTF8String();
                    names.Add(str);
                }

                pos += length;
            }

        }

        protected override void ParseHeader(Span<byte> header, Span<byte> everything)
        {
            length = header.GetBigEndianDWORD(0);
        }
        int length;
    }
}
