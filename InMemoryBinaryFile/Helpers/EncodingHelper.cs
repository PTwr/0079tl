using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryBinaryFile.Helpers
{
    public static class EncodingHelper
    {
        static EncodingHelper()
        {
            //inits System.Text.Encoding.CodePages package to access more codepages
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        class EncodingStringComparer : StringComparer
        {
            public override int Compare(string? x, string? y)
            {
                x = SimplifyEncodingName(x);

                y = SimplifyEncodingName(y);

                return string.Compare(x, y);
            }

            public override bool Equals(string? x, string? y)
            {
                x = SimplifyEncodingName(x);

                y = SimplifyEncodingName(y);

                return x == y;
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

            public override int GetHashCode(string obj)
            {
                return SimplifyEncodingName(obj).GetHashCode();
            }
        }
        public static bool TryParse(string s, out Encoding encoding)
        {
            EncodingInfo ei;
            encoding = null;

            var all = Encoding.GetEncodings().Distinct();

            var byDisplayName = all
                .GroupBy(x => x.DisplayName)
                .Select(x => x.First())
                .ToDictionary(i => i.DisplayName, i => i, new EncodingStringComparer());
            var byCodepage = all.ToDictionary(i => i.CodePage.ToString(), i => i, new EncodingStringComparer());
            var byName = all.ToDictionary(i => i.Name, i => i, new EncodingStringComparer());

            if (byDisplayName.TryGetValue(s, out ei))
            {
                encoding = ei.GetEncoding();
                return true;
            }
            if (byCodepage.TryGetValue(s, out ei))
            {
                encoding = ei.GetEncoding();
                return true;
            }
            if (byName.TryGetValue(s, out ei))
            {
                encoding = ei.GetEncoding();
                return true;
            }

            return false;
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
