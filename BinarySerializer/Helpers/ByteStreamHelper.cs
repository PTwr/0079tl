
namespace BinarySerializer.Helpers
{
    public static class ByteStreamHelper
    {
        public static IEnumerable<byte> ToBigEndianBytes(this IEnumerable<ushort> words)
        {
            return words.SelectMany(i => i.GetBigEndianBytes());
        }
        public static IEnumerable<byte> ToBigEndianBytes(this IEnumerable<uint> words)
        {
            return words.SelectMany(i => i.GetBigEndianBytes());
        }

        public static IEnumerable<byte> ToBigEndianBytes(this IEnumerable<short> words)
        {
            return words.SelectMany(i => i.GetBigEndianBytes());
        }
        public static IEnumerable<byte> ToBigEndianBytes(this IEnumerable<int> words)
        {
            return words.SelectMany(i => i.GetBigEndianBytes());
        }

        public static bool Matches(this Span<byte> data, Span<byte?> pattern, Index index)
        {
            var patternPos = index.GetOffset(data.Length);

            //can't fit
            if (data.Length < patternPos + pattern.Length) return false;

            for (int i = 0; i < pattern.Length; i++)
            {
                //null is wildcard
                if (pattern[i] == null) continue;

                if (data[patternPos + i] != pattern[i]) return false;
            }

            return true;
        }
        public static bool Matches(this IEnumerable<byte> data, IEnumerable<byte?> pattern, Index index)
        {
            var patternPos = index.GetOffset(data.Count());

            //can't fit
            if (data.Count() < pattern.Count() + pattern.Count()) return false;

            for (int i = 0; i < pattern.Count(); i++)
            {
                //null is wildcard
                if (pattern.ElementAt(i) == null) continue;

                if (data.ElementAt(patternPos + i) != pattern.ElementAt(i)) return false;
            }

            return true;
        }

        public static bool StartsWith(this IEnumerable<byte> data, IEnumerable<byte?> pattern)
        {
            return data.Matches(pattern, new Index(0, false));
        }
        public static bool EndsWith(this IEnumerable<byte> data, IEnumerable<byte?> pattern)
        {
            return data.Matches(pattern, new Index(1, true));
        }

        public static bool StartsWith(this Span<byte> data, Span<int> pattern)
        {
            var p = pattern.ToArray().Select(i => i < 0 ? null : (byte?)i).ToArray();
            return data.Matches(p.AsSpan(), new Index(0, false));
        }
        public static bool EndsWith(this Span<byte> data, Span<int> pattern)
        {
            var p = pattern.ToArray().Select(i => i < 0 ? null : (byte?)i).ToArray();
            return data.Matches(p.AsSpan(), new Index(1, true));
        }

        public static bool StartsWith(this Span<byte> data, Span<byte?> pattern)
        {
            return data.Matches(pattern, new Index(0, false));
        }
        public static bool EndsWith(this Span<byte> data, Span<byte?> pattern)
        {
            return data.Matches(pattern, new Index(1, true));
        }
    }
}
