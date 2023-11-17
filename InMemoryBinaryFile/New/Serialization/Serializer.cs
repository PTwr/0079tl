using InMemoryBinaryFile.New.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryBinaryFile.New.Serialization
{
    public static class Serializer
    {
        //public static byte[] Serialize(object source, List<(int absolute, int header, int body)> offsetsHistory, out int segmentLength)
        //{

        //}
        //public static byte[] Serialize<T>(T source)
        //    where T : IBinarySegment
        //{
        //    return (T)_Serialize(target, [], out _);
        //}
    }
    public static class Helpers
    {
        //public static void Set<T>(this List<T> list, int index, IEnumerable<T> collection)
        //{
        //    var items = collection.
        //    int count = collection.Count();
        //    int end = index + count - 1;
        //    int n = 0;
        //    for (int i = index; i < list.Count; i++)
        //    {
        //        list[i] = collection[n];
        //    }
        //}
        //public static void Pad<T>(this List<T> list, int length)
        //{
        //    if ()

        //    var c = length - list.Count();
        //    list.AddRange(Enumerable.Repeat(default(T)!, c));
        //}
    }
}
