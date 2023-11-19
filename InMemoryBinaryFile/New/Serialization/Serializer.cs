using InMemoryBinaryFile.Helpers;
using InMemoryBinaryFile.New.Attributes;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryBinaryFile.New.Serialization
{
    public static class Serializer
    {
        private static List<byte> _Serialize(object source, List<(int absolute, int header, int body)> offsetsHistory, List<byte>? result, out int segmentLength)
        {
            BinarySegmentAttribute BinarySegmentAttribute = source.GetType().GetCustomAttribute<BinarySegmentAttribute>()!;

            if (BinarySegmentAttribute == null)
            {
                throw new Exception($"{nameof(Attributes.BinarySegmentAttribute)} is required");
            }

            //plop root offsets at start of stack
            if (!offsetsHistory.Any())
            {
                offsetsHistory.Add((0, BinarySegmentAttribute.HeaderOffset, BinarySegmentAttribute.BodyOffset));
            }
            if (result == null)
            {
                result = new List<byte>();
            }

            foreach ((var BinaryFieldAttribute, var prop) in source.Properties()
                .WithAttribute<BinaryFieldAttribute>()
                .Where(i => i.attr.GetIf(source, i.prop))
                .OrderBy(i => i.attr.SerializationOrder ?? i.attr.Order)
                )
            {
                IEnumerable<byte> fieldBytes = null;

                var encoding = (BinaryFieldAttribute as StringEncodingAttribute)?.GetEncoding(source, prop.Name) ?? Encoding.ASCII;

                var NullTerminatedStringAttribute = BinaryFieldAttribute as NullTerminatedStringAttribute;
                var FixedLengthStringAttribute = BinaryFieldAttribute as FixedLengthStringAttribute;

                (int pos, int length, int count) = source.GetFieldMetadata(prop, BinaryFieldAttribute, offsetsHistory);

                if (prop.PropertyType == typeof(byte))
                {
                    var x = (byte)prop.GetValue(source)!;
                    result.Emplace(pos, [x]);
                }
                else if (prop.PropertyType == typeof(ushort))
                {
                    var x = (ushort)prop.GetValue(source)!;
                    fieldBytes = BinaryFieldAttribute.LittleEndian ?
                        x.GetLittleEndianBytes() : x.GetBigEndianBytes();
                }
                else if (prop.PropertyType == typeof(short))
                {
                    var x = (short)prop.GetValue(source)!;
                    fieldBytes = BinaryFieldAttribute.LittleEndian ?
                        x.GetLittleEndianBytes() : x.GetBigEndianBytes();
                }
                else if (prop.PropertyType == typeof(uint))
                {
                    var x = (uint)prop.GetValue(source)!;
                    fieldBytes = BinaryFieldAttribute.LittleEndian ?
                        x.GetLittleEndianBytes() : x.GetBigEndianBytes();
                }
                else if (prop.PropertyType == typeof(int))
                {
                    var x = (int)prop.GetValue(source)!;
                    fieldBytes = BinaryFieldAttribute.LittleEndian ?
                        x.GetLittleEndianBytes() : x.GetBigEndianBytes();
                }
                else if (prop.PropertyType == typeof(string) && NullTerminatedStringAttribute != null)
                {
                    var x = (string)prop.GetValue(source)!;
                    fieldBytes = x.ToBytes(encoding, true);
                }
                else if (prop.PropertyType == typeof(string) && FixedLengthStringAttribute != null)
                {
                    var x = (string)prop.GetValue(source)!;
                    fieldBytes = x.ToBytes(encoding, false, length);
                }
                else if (prop.PropertyType == typeof(string[]) && NullTerminatedStringAttribute != null)
                {
                    var x = (string[])prop.GetValue(source)!;
                    fieldBytes = x.SelectMany(s => s.ToBytes(encoding, true));
                }
                else if (prop.PropertyType == typeof(List<string>) && NullTerminatedStringAttribute != null)
                {
                    var x = (List<string>)prop.GetValue(source)!;
                    fieldBytes = x.SelectMany(s => s.ToBytes(encoding, true));
                }
                else if (prop.PropertyType == typeof(Dictionary<int, string>) && NullTerminatedStringAttribute != null)
                {
                    var x = (Dictionary<int, string>)prop.GetValue(source)!;
                    fieldBytes = x.SelectMany(s => s.Value.ToBytes(encoding, true));
                }
                else if (prop.IsAssignableTo<IBinarySegment>())
                {
                    var x = (IBinarySegment)prop.GetValue(source)!;

                    var ChildBinarySegmentAttribute = x.GetType().GetCustomAttribute<BinarySegmentAttribute>();

                    _Serialize(x, [(pos, ChildBinarySegmentAttribute?.HeaderOffset ?? 0, ChildBinarySegmentAttribute?.BodyOffset ?? 0), .. offsetsHistory], result, out _);
                }
                else if (prop.IsAssignableTo<IBinarySegment[]>() || prop.IsAssignableTo<IEnumerable<IBinarySegment>>())
                {
                    IEnumerable<IBinarySegment> items = (prop.GetValue(source) as IEnumerable<IBinarySegment>)!;

                    int childOffset = pos;

                    foreach (var x in items)
                    {
                        var ChildBinarySegmentAttribute = x.GetType().GetCustomAttribute<BinarySegmentAttribute>();

                        _Serialize(x, [(childOffset, ChildBinarySegmentAttribute?.HeaderOffset ?? 0, ChildBinarySegmentAttribute?.BodyOffset ?? 0), .. offsetsHistory], result, out var itemLength);

                        childOffset += itemLength;
                    }
                }
                else if (prop.PropertyType == typeof(byte[]))
                {
                    fieldBytes = (byte[])prop.GetValue(source)!;
                }

                if (fieldBytes != null)
                {
                    result.Emplace(pos, fieldBytes);
                }
            }

            segmentLength = BinarySegmentAttribute.GetLength(source);
            return result;
        }
        public static List<byte> Serialize<T>(T source)
            where T : IBinarySegment
        {
            return _Serialize(source, [], [], out _);
        }
    }
    public static class Helpers
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
