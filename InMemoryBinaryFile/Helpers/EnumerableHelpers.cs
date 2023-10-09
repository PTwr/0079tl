using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryBinaryFile.Helpers
{
    public static class EnumerableHelpers
    {
        public static IEnumerable<T> ConcatIf<T>(this IEnumerable<T> enumerable, IEnumerable<T> values, bool condition)
        {
            if (condition)
                return enumerable.Concat(values);
            else
                return enumerable;
        }
        public static IEnumerable<T> ConcatIf<T>(this IEnumerable<T> enumerable, T value, bool condition)
        {
            if (condition)
                return enumerable.Concat(new T[] { value });
            else
                return enumerable;
        }
    }
}
