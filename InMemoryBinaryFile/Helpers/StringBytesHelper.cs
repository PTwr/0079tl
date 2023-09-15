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
