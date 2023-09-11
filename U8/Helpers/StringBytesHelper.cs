using System.Xml;

namespace U8.Helpers
{
    public static class StringBytesHelper
    {
        public static Span<byte> FindNullTerminator(this Span<byte> buffer)
        {
            int l = 0;
            while (l < buffer.Length && buffer[l] != 0)
            {
                l++;
            }

            return buffer.Slice(0, l);
        }

        public static Span<byte> FindNullTerminator(this byte[] buffer)
        {
            return FindNullTerminator(buffer.AsSpan());
        }

        public static string AsciiBytesToString(this Span<byte> buffer)
        {
            return System.Text.Encoding.ASCII.GetString(buffer);
        }
        public static string UTF8BytesToString(this Span<byte> buffer)
        {
            return System.Text.Encoding.UTF8.GetString(buffer);
        }
    }
}