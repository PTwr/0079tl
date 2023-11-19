using InMemoryBinaryFile.Helpers;
using InMemoryBinaryFile.New.Attributes;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryBinaryFile.New.Serialization
{
    public static class Deserializer
    {
        public static IEnumerable<PropertyInfo> Properties(this object obj, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            return obj.GetType().GetProperties(bindingFlags);
        }
        public static IEnumerable<(Tattrib attr, PropertyInfo prop)> WithAttribute<Tattrib>(this IEnumerable<PropertyInfo> props, bool inherit = false)
            where Tattrib : Attribute
        {
            foreach (var prop in props)
            {
                var attrib = prop.GetCustomAttribute<Tattrib>(inherit);
                if (attrib != null)
                {
                    yield return (attrib, prop);
                }
            }
        }
        private static object _Deserialize(Span<byte> bytes, object target, List<(int absolute, int header, int body)> offsetsHistory, out int segmentLength)
        {
            BinarySegmentAttribute BinarySegmentAttribute = target.GetType().GetCustomAttribute<BinarySegmentAttribute>()!;

            if (BinarySegmentAttribute == null)
            {
                throw new Exception($"{nameof(Attributes.BinarySegmentAttribute)} is required");
            }

            //plop root offsets at start of stack
            if (!offsetsHistory.Any())
            {
                offsetsHistory.Add((0, BinarySegmentAttribute.HeaderOffset, BinarySegmentAttribute.BodyOffset));
            }

            foreach ((var BinaryFieldAttribute, var prop) in target.Properties()
                .WithAttribute<BinaryFieldAttribute>()
                .Where(i => i.attr.GetIf(target, i.prop))
                .OrderBy(i=>i.attr.Order)
                )
            {
                var encoding = (BinaryFieldAttribute as StringEncodingAttribute)?.GetEncoding(target, prop.Name) ?? Encoding.ASCII;

                var NullTerminatedStringAttribute = BinaryFieldAttribute as NullTerminatedStringAttribute;
                var FixedLengthStringAttribute = BinaryFieldAttribute as FixedLengthStringAttribute;

                object? newValue = null;

                (int pos, int length, int count) = target.GetFieldMetadata(prop, BinaryFieldAttribute, offsetsHistory);
                var slice = bytes.Segment(pos, length);

                if (prop.PropertyType == typeof(byte))
                {
                    newValue = slice[0];
                }
                else if (prop.PropertyType == typeof(ushort))
                {
                    newValue = BinaryFieldAttribute.LittleEndian ?
                        BinaryPrimitives.ReadUInt16LittleEndian(slice) :
                        BinaryPrimitives.ReadUInt16BigEndian(slice);
                }
                else if (prop.PropertyType == typeof(short))
                {
                    newValue = BinaryFieldAttribute.LittleEndian ?
                        BinaryPrimitives.ReadInt16LittleEndian(slice) :
                        BinaryPrimitives.ReadInt16BigEndian(slice);
                }
                else if (prop.PropertyType == typeof(uint))
                {
                    newValue = BinaryFieldAttribute.LittleEndian ?
                        BinaryPrimitives.ReadUInt32LittleEndian(slice) :
                        BinaryPrimitives.ReadUInt32BigEndian(slice);
                }
                else if (prop.PropertyType == typeof(int))
                {
                    newValue = BinaryFieldAttribute.LittleEndian ?
                        BinaryPrimitives.ReadInt32LittleEndian(slice) :
                        BinaryPrimitives.ReadInt32BigEndian(slice);
                }
                else if (prop.PropertyType == typeof(string) && NullTerminatedStringAttribute != null)
                {
                    newValue = slice
                        .FindNullTerminator()
                        .ToDecodedString(encoding);
                }
                else if (prop.PropertyType == typeof(string) && FixedLengthStringAttribute != null)
                {
                    newValue = slice
                        .Slice(0, FixedLengthStringAttribute.Length)
                        .ToDecodedString(encoding);
                }
                else if (prop.PropertyType == typeof(string[]) && NullTerminatedStringAttribute != null)
                {
                    newValue = slice
                        .ToDecodedNullTerminatedStrings(encoding, count < 0 ? null : count)
                        .ToArray();
                }
                else if (prop.PropertyType == typeof(List<string>) && NullTerminatedStringAttribute != null)
                {
                    newValue = slice
                        .ToDecodedNullTerminatedStrings(encoding, count < 0 ? null : count);
                }
                else if (prop.PropertyType == typeof(Dictionary<int, string>) && NullTerminatedStringAttribute != null)
                {
                    newValue = slice
                        .ToDecodedNullTerminatedStringDict(encoding, count < 0 ? null : count);
                }
                else if (prop.IsAssignableTo<IBinarySegment>())
                {
                    object? item = prop.CreatePropertyObject(target);
                    if (item != null)
                    {
                        var ChildBinarySegmentAttribute = item.GetType().GetCustomAttribute<BinarySegmentAttribute>();
                        newValue = _Deserialize(bytes, item, [(pos, ChildBinarySegmentAttribute?.HeaderOffset ?? 0, ChildBinarySegmentAttribute?.BodyOffset ?? 0), .. offsetsHistory], out _);
                    }
                }
                else if (prop.IsAssignableTo<IBinarySegment[]>() || prop.IsAssignableTo<IEnumerable<IBinarySegment>>())
                {
                    List<New.IBinarySegment> items = new List<New.IBinarySegment>();
                    int childOffset = pos;
                    int n = 0;
                    while (childOffset < pos + slice.Length)
                    {
                        var item = prop.CreateCollectionItem(target);
                        if (item != null)
                        {
                            var ChildBinarySegmentAttribute = item.GetType().GetCustomAttribute<BinarySegmentAttribute>();
                            item = _Deserialize(bytes, item, [(childOffset, ChildBinarySegmentAttribute?.HeaderOffset ?? 0, ChildBinarySegmentAttribute?.BodyOffset ?? 0), .. offsetsHistory], out var itemLength);
                            childOffset += itemLength;
                            items.Add((IBinarySegment)item);
                        }
                        n++;

                        if (count >= 0 && n >= count)
                        { break; }
                    }
                    if (prop.IsAssignableTo<IBinarySegment[]>())
                    {
                        newValue = items.CastToArray(prop.PropertyType.GetCollectionType()!);
                    }
                    else if (prop.IsAssignableTo<IEnumerable<IBinarySegment>>())
                    {
                        newValue = items.CastToList(prop.PropertyType.GetCollectionType()!);
                    }
                }
                else if (prop.PropertyType == typeof(byte[]))
                {
                    newValue = slice.ToArray();
                }

                if (newValue != null)
                {
                    if (BinaryFieldAttribute.ExpectedValue != null)
                    {
                        if (!newValue.Equals(BinaryFieldAttribute.ExpectedValue))
                        {
                            throw new Exception($"Invalid value for {prop.Name}, expected {BinaryFieldAttribute.ExpectedValue} actual {newValue}");
                        }
                    }

                    prop.SetValue(target, newValue);
                }
            }

            segmentLength = BinarySegmentAttribute.GetLength(target);

            if (target is IPostProcessing)
            {
                ((IPostProcessing)target).AfterDeserialization();
            }

            return target;
        }

        public static T Deserialize<T>(Span<byte> bytes, T target)
            where T : IBinarySegment
        {
            return (T)_Deserialize(bytes, target, [], out _);
        }

        public static T Deserialize<T>(Span<byte> bytes)
            where T : IBinarySegment, new()
        {
            T t = new T();

            return Deserialize(bytes, t);
        }
    }
}
