using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinarySerializer.Helpers
{
    public static class CollectionHelper
    {
        public static void Emplace<T>(this List<T> list, int index, IEnumerable<T> collection)
        {
            int count = collection.Count();
            int end = index + count;
            list.PadToLength(end);

            for (int n = 0; n < collection.Count(); n++)
            {
                list[index + n] = collection.ElementAt(n);
            }
        }
        public static void PadToLength<T>(this List<T> list, int length)
        {
            var c = length - list.Count();
            if (c < 0)
            {
                return;
            }

            list.AddRange(Enumerable.Repeat(default(T)!, c));
        }
    }
}
