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
        public static Span<byte> FindNullTerminatedString(this byte[] buffer)
        {
            return FindNullTerminator(buffer.AsSpan());
        }

        public static string ToW1250String(this Span<byte> buffer)
        {
            return EncodingHelper.Windows1250.GetString(buffer);
        }
        public static string ToShiftJisString(this Span<byte> buffer)
        {
            return EncodingHelper.Shift_JIS.GetString(buffer);
        }
        public static string ToAsciiString(this Span<byte> buffer)
        {
            return System.Text.Encoding.ASCII.GetString(buffer);
        }
        public static string ToUTF8String(this Span<byte> buffer)
        {
            return System.Text.Encoding.UTF8.GetString(buffer);
        }

        public static byte[] ToUTF8Bytes(this string text, bool appendNullTerminator = false)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(text);

            if (appendNullTerminator)
            {
                return bytes.Concat(NullTerminator).ToArray();
            }
            return bytes.ToArray();
        }
        public static byte[] ToASCIIBytes(this string text, bool appendNullTerminator = false)
        {
            var bytes = System.Text.Encoding.ASCII.GetBytes(text);

            if (appendNullTerminator)
            {
                return bytes.Concat(NullTerminator).ToArray();
            }
            return bytes.ToArray();
        }
        public static byte[] ToW1250Bytes(this string text, bool appendNullTerminator = false)
        {
            var bytes = EncodingHelper.Windows1250.GetBytes(text);

            if (appendNullTerminator)
            {
                return bytes.Concat(NullTerminator).ToArray();
            }
            return bytes.ToArray();
        }
        private static byte[] NullTerminator = new byte[] { 0 };
}
}
