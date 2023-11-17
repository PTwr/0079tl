using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryBinaryFile.Helpers
{
    public static class StringBytesHelper
    {
        public static Span<byte> FindNullTerminator(this Span<byte> buffer, int start = 0)
        {
            buffer = start > 0 ? buffer.Slice(start) : buffer;
            int l = 0;
            while (l < buffer.Length && buffer[l] != 0)
            {
                l++;
            }

            return buffer.Slice(0, l);
        }
        public static Span<byte> FindNullTerminatedString(this byte[] buffer, int start = 0)
        {
            return FindNullTerminator(buffer.AsSpan(), start);
        }

        public static string ToDecodedString(this Span<byte> buffer, Encoding encoding)
        {
            return encoding.GetString(buffer);
        }
        public static List<string> ToDecodedNullTerminatedStrings(this Span<byte> buffer, Encoding encoding, int? count = null)
        {
            return buffer.ToDecodedNullTerminatedStringDict(encoding, count).Values.ToList();
        }
        public static Dictionary<int, string> ToDecodedNullTerminatedStringDict(this Span<byte> buffer, Encoding encoding, int? count = null)
        {
            Dictionary<int, string> result = new();
            for (int start = 0; start < buffer.Length;)
            {
                //series of null terminated strings
                var s = buffer.Slice(start).FindNullTerminator();
                var ss = s.ToDecodedString(encoding);

                result[start] = ss;

                start += s.Length + 1;

                if (count.HasValue && result.Count >= count)
                {
                    break;
                }
            }

            return result;
        }

        public static string ToW1250String(this Span<byte> buffer)
        {
            return buffer.ToDecodedString(EncodingHelper.Windows1250);
        }
        public static string ToShiftJisString(this Span<byte> buffer)
        {
            return buffer.ToDecodedString(EncodingHelper.Shift_JIS);
        }
        public static string ToAsciiString(this Span<byte> buffer)
        {
            return buffer.ToDecodedString(System.Text.Encoding.ASCII);
        }
        public static string ToUTF8String(this Span<byte> buffer)
        {
            return buffer.ToDecodedString(System.Text.Encoding.UTF8);
        }

        public static byte[] ToBytes(this List<string> texts, Encoding encoding, bool appendNullTerminator = false)
        {
            return texts.SelectMany(i => i.ToBytes(encoding, appendNullTerminator)).ToArray();
        }
        public static byte[] ToBytes(this Dictionary<int, string> texts, Encoding encoding, bool appendNullTerminator = false)
        {
            return texts.SelectMany(i => i.Value.ToBytes(encoding, appendNullTerminator)).ToArray();
        }

        public static byte[] ToBytes(this string text, Encoding encoding, bool appendNullTerminator = false)
        {
            var bytes = encoding.GetBytes(text);

            if (appendNullTerminator)
            {
                return bytes.Concat(NullTerminator).ToArray();
            }
            return bytes.ToArray();
        }

        public static byte[] ToUTF8Bytes(this string text, bool appendNullTerminator = false)
        {
            return text.ToBytes(System.Text.Encoding.UTF8, appendNullTerminator);
        }
        public static byte[] ToASCIIBytes(this string text, bool appendNullTerminator = false)
        {
            return text.ToBytes(System.Text.Encoding.ASCII, appendNullTerminator);
        }
        public static byte[] ToW1250Bytes(this string text, bool appendNullTerminator = false)
        {
            return text.ToBytes(EncodingHelper.Windows1250, appendNullTerminator);
        }
        public static byte[] ToShiftJisBytes(this string text, bool appendNullTerminator = false)
        {
            return text.ToBytes(EncodingHelper.Shift_JIS, appendNullTerminator);
        }
        private static byte[] NullTerminator = new byte[] { 0 };
}
}
