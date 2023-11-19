using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

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
                if (lineLength > 0 && (i + 1) % lineLength == 0 && (i + 1) != count)
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

        /// <summary>
        /// Shitty Lua minifier
        /// Build to snatch some more space in 0079 lua scripts to avoid patchign arc with longer files
        /// </summary>
        /// <param name="lua"></param>
        /// <returns></returns>
        public static string MinifyLua(this string lua)
        {
            Regex blockComment = new Regex(@"--\[(=*)\[(.|\n)*?\]\1\]");
            Regex inlineComment = new Regex(@"--.*");

            var removedComments = blockComment.Replace(lua, string.Empty);
            removedComments = inlineComment.Replace(removedComments, string.Empty);

            var lines = removedComments
                .Split('\x0D', '\x0A');

            //todo make/use minifier which doesnt break on multiline comment
            var trimmedLines = lines
                .Select(i => i.Trim()) //ignore formatting
                .Select(i => i.Replace(" = ", "=")) // unnecessary spaces in middle of dict lines
                .Select(i => i.Replace(" ~= ", "~=")) // unnecessary spaces
                .Select(i => i.Replace(" ~= ", "~=")) // unnecessary spaces
                .Select(i => i.Replace(") )", "))")) // unnecessary spaces
                .Select(i => i.Replace("( (", "((")) // unnecessary spaces
                .Where(i => !i.StartsWith("--")) //skip comments
                .Where(i => !string.IsNullOrWhiteSpace(i)) //skip empty lines
                ;

            var trimmedscript = string.Join("\x0D\x0A", trimmedLines);

            return trimmedscript;
        }
    }
}
