using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEVLib.Helpers
{
    public static class BinarySegmentHelper
    {
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
        public static IEnumerable<byte> PadRight(this IEnumerable<byte> bytes, int count, byte padValue = 0)
        {
            return bytes.Concat(Enumerable.Repeat(padValue, count));
        }
        public static IEnumerable<byte> PadLeft(this IEnumerable<byte> bytes, int count, byte padValue = 0)
        {
            return Enumerable.Repeat(padValue, count).Concat(bytes);
        }

        public static IEnumerable<byte> PadToAlignment(this IEnumerable<byte> bytes, int alignmentBytes)
        {
            var misalignedBy = bytes.Count() % alignmentBytes;
            var missingZeros = (alignmentBytes - misalignedBy) % 4;

            return bytes.PadRight(missingZeros, 0);
        }

        public static byte[] SubArray(this byte[] bytes, int from, int to)
        {
            return bytes.Skip(from).Take(to - from).ToArray();
        }

        public static byte[] GetBigEndianBytes(this short n)
        {
            return BitConverter.GetBytes(BinaryPrimitives.ReverseEndianness(n));
        }
        public static byte[] GetBigEndianBytes(this ushort n)
        {
            return BitConverter.GetBytes(BinaryPrimitives.ReverseEndianness(n));
        }
        public static byte[] GetBigEndianBytes(this int n)
        {
            return BitConverter.GetBytes(BinaryPrimitives.ReverseEndianness(n));
        }
        public static byte[] GetBigEndianBytes(this uint n)
        {
            return BitConverter.GetBytes(BinaryPrimitives.ReverseEndianness(n));
        }
        public static IEnumerable<T> Concatenate<T>(params IEnumerable<T>[] lists)
        {
            return lists.SelectMany(x => x);
        }
    }
}
