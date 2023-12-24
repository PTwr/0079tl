using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace BinarySerializer.Helpers
{
    public static class EncodingHelper
    {
        public static IEnumerable<Encoding> EncodingGuestimator(string expectedString, byte[] rawBytes)
        {
            //required for .NET Core
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            foreach (var ei in Encoding.GetEncodings())
            {
                var e = ei.GetEncoding();
                var str = e.GetString(rawBytes);

                if (str == expectedString)
                {
                    yield return e;
                }
            }
        }

        static List<Encoding> allEncodings;
        static EncodingHelper()
        {
            //inits System.Text.Encoding.CodePages package to access more codepages
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            allEncodings = Encoding.GetEncodings().Select(i=>i.GetEncoding()).ToList();
        }
        private static string SimplifyEncodingName(string? x)
        {
            x = x.ToLower()
                .Replace("_", "")
                .Replace("-", "")
                .Replace(" ", "")
                .Trim();
            return x;
        }
        public static bool TryParse(string s, out Encoding? encoding)
        {
            s = SimplifyEncodingName(s);

            encoding = allEncodings.FirstOrDefault(i => i.CodePage.ToString() == s) ??
                allEncodings.FirstOrDefault(i => SimplifyEncodingName(i.EncodingName) == s) ??
                allEncodings.FirstOrDefault(i => SimplifyEncodingName(i.BodyName) == s) ??
                allEncodings.FirstOrDefault(i => SimplifyEncodingName(i.HeaderName) == s) ??
                allEncodings.FirstOrDefault(i => SimplifyEncodingName(i.WebName) == s);

            return encoding != null;
        }

        private static Encoding? shiftJIS;
        public static Encoding Shift_JIS
        {
            get
            {
                if (shiftJIS != null)
                {
                    return shiftJIS;
                }

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

        public static void EnableUTF8Console()
        {
            //required to writeline funny japanese hieroglyphs without breaking other text
            Console.OutputEncoding = Encoding.GetEncoding("utf-8");
        }
        public static void EnableUTF16Console()
        {
            //required to writeline funny japanese hieroglyphs without breaking other text
            Console.OutputEncoding = Encoding.GetEncoding("utf-16");
        }
        public static void EnableShiftJistConsole()
        {
            //required to writeline funny japanese hieroglyphs without breaking other text
            Console.OutputEncoding = Shift_JIS;
        }
        public static void ConsoleEncoding(Encoding encoding)
        {
            //required to writeline funny japanese hieroglyphs without breaking other text
            Console.OutputEncoding = encoding;
        }

    }
}
