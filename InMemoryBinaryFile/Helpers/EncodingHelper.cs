using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryBinaryFile.Helpers
{
    public static class EncodingHelper
    {
        private static Encoding? shiftJIS;
        public static Encoding Shift_JIS
        {
            get
            {
                if (shiftJIS != null)
                {
                    return shiftJIS;
                }

                //inits System.Text.Encoding.CodePages package to access more codepages
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                shiftJIS = Encoding.GetEncoding("shift_jis");
                return shiftJIS;
            }
        }

        private static Encoding? w1250;
        public static Encoding Windows1250
        {
            get
            {
                if (w1250 != null)
                {
                    return w1250;
                }

                //inits System.Text.Encoding.CodePages package to access more codepages
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                w1250 = Encoding.GetEncoding(1250);
                return w1250;
            }
        }

        public static void EnableShiftJistConsole()
        {
            //required to writeline funny japanese hieroglyphs without breaking other text
            Console.OutputEncoding = Encoding.GetEncoding("utf-16");
        }
    }
}
