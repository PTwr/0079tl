using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryBinaryFile.Helpers
{
    public static class StringHelper
    {
        public static string Intend(this string str, int depth = 2, string intendation = " ")
        {
            var intend = "";
            for (int i = 0; i < depth; i++) intend += intendation;

            var result = string.Join(Environment.NewLine, str
                .Split(Environment.NewLine)
                .Select(s => intend + s)
                );

            return result;
        }

        public static string ToHexString(this IEnumerable<byte> bytes, int lineLength = 16)
        {
            var count = bytes.Count();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                sb.Append(bytes.ElementAt(i).ToString("X2") + " ");
                if ((i + 1) % lineLength == 0 && (i + 1) != count)
                {
                    sb.AppendLine();
                }
                else if ((i + 1) % 4 == 0 && (i + 1) != count)
                {
                    sb.Append(" | ");
                }
            }
            return sb.ToString();
        }
    }
}
