using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using BinarySerializer.Annotation;
using BinarySerializer.Helpers;

namespace BinarySerializer
{
    public static class Serializer
    {
        private static byte[] _SerializePrimitive(object obj, (int pos, int length, int count, bool littleEndian, int alignment, bool nullTerminated, Encoding encoding) metadata)
        {
            if (obj == null)
            {
                return [];
            }

            var itemType = obj.GetType();

            if (itemType == typeof(byte))
            {
                return [(byte)obj];
            }
            else if (itemType == typeof(ushort))
            {
                var x = (ushort)obj!;
                return metadata.littleEndian ?
                    x.GetLittleEndianBytes() : x.GetBigEndianBytes();
            }
            else if (itemType == typeof(short))
            {
                var x = (short)obj!;
                return metadata.littleEndian ?
                    x.GetLittleEndianBytes() : x.GetBigEndianBytes();
            }
            else if (itemType == typeof(uint))
            {
                var x = (uint)obj!;
                return metadata.littleEndian ?
                    x.GetLittleEndianBytes() : x.GetBigEndianBytes();
            }
            else if (itemType == typeof(int))
            {
                var x = (int)obj!;
                return metadata.littleEndian ?
                    x.GetLittleEndianBytes() : x.GetBigEndianBytes();
            }
            else if (itemType == typeof(string) && metadata.nullTerminated)
            {
                var x = (string)obj!;
                if (metadata.nullTerminated)
                {
                    return x.ToBytes(metadata.encoding, true);
                }
                else
                {
                    return x
                        .ToBytes(metadata.encoding, false, metadata.length)
                        .PadToAlignment(metadata.alignment)
                        .ToArray();
                }
            }

            throw new Exception($"Unsupported primitive type '{itemType.Name}'");
        }
        private static object _DeserializePrimitive(Type itemType, Span<byte> bytes, out int itemByteLength, (int pos, int length, int count, bool littleEndian, int alignment, bool nullTerminated, Encoding encoding) metadata)
        {
            if (itemType == typeof(byte))
            {
                itemByteLength = 1;
                return bytes[0];
            }
            else if (itemType == typeof(ushort))
            {
                itemByteLength = 2;
                return metadata.littleEndian ?
                    BinaryPrimitives.ReadUInt16LittleEndian(bytes) :
                    BinaryPrimitives.ReadUInt16BigEndian(bytes);
            }
            else if (itemType == typeof(short))
            {
                itemByteLength = 2;
                return metadata.littleEndian ?
                    BinaryPrimitives.ReadInt16LittleEndian(bytes) :
                    BinaryPrimitives.ReadInt16BigEndian(bytes);
            }
            else if (itemType == typeof(uint))
            {
                itemByteLength = 4;
                return metadata.littleEndian ?
                    BinaryPrimitives.ReadUInt32LittleEndian(bytes) :
                    BinaryPrimitives.ReadUInt32BigEndian(bytes);
            }
            else if (itemType == typeof(int))
            {
                itemByteLength = 4;
                return metadata.littleEndian ?
                    BinaryPrimitives.ReadInt32LittleEndian(bytes) :
                    BinaryPrimitives.ReadInt32BigEndian(bytes);
            }
            else if (itemType == typeof(string))
            {
                if (metadata.nullTerminated)
                {
                    bytes = bytes.FindNullTerminator();
                    itemByteLength = bytes.Length + 1;
                }
                else
                {
                    bytes.Slice(0, metadata.length);
                    itemByteLength = bytes.Length;
                }
                return bytes.ToDecodedString(metadata.encoding);
            }

            throw new Exception($"Unsupported primitive type '{itemType.Name}'");
        }

        private static object _Deserialize(Span<byte> bytes, object target, List<(int absolute, int header, int body)> offsetsHistory, out int segmentLength)
        {
            var BinarySegmentAttribute = target.GetType().GetCustomAttribute<BinarySegmentAttribute>()!;

            if (BinarySegmentAttribute == null)
            {
                throw new Exception($"{nameof(BinarySegmentAttribute)} is required");
            }

            //plop root offsets at start of stack
            if (!offsetsHistory.Any())
            {
                offsetsHistory.Add((0, BinarySegmentAttribute.HeaderOffset, BinarySegmentAttribute.BodyOffset));
            }

            foreach ((var BinaryFieldAttribute, var prop) in target.Properties()
                .WithAttribute<BinaryFieldAttribute>(inherit: true)
                .OrderBy(i => OrderAttribute.GetSerializationOrder(target, i.prop))
                )
            {
                object? newValue = null;

                var metadata = target.GetFieldMetadata(prop, BinaryFieldAttribute, offsetsHistory);
                (int pos, int length, int count, _, _, _, _) = metadata;
                var slice = bytes.Segment(pos, length);

                //can't be prefiltered during deserialization as it can change depending on previously deserialized fields
                if (!DeserializeIfAttribute.If(target, prop, slice))
                {
                    continue;
                }

                var implementationType = DeserializeAsAttribute.GetTargetType(target, prop, slice);

                //nested binary segments
                if (prop.IsAssignableTo<IBinarySegment>())
                {
                    object? item = prop.CreatePropertyObject(target, implementationType);
                    if (item != null)
                    {
                        var ChildBinarySegmentAttribute = item.GetType().GetCustomAttribute<BinarySegmentAttribute>();
                        newValue = _Deserialize(prop.HasAttribute<NestedFileAttribute>() ? slice : bytes,
                                                item,
                                                prop.HasAttribute<NestedFileAttribute>() ? [] :
                                                [(pos, ChildBinarySegmentAttribute?.HeaderOffset ?? 0, ChildBinarySegmentAttribute?.BodyOffset ?? 0), .. offsetsHistory],
                                                out _);
                    }
                }
                else if (prop.IsAssignableTo<IBinarySegment[]>() || prop.IsAssignableTo<IEnumerable<IBinarySegment>>())
                {
                    Dictionary<int, IBinarySegment> items = new();
                    int itemOffset = 0;
                    int n = 0;
                    while (itemOffset < slice.Length)
                    {
                        if (!CollectionAttribute.ShouldContinueDeserialization(target, prop, items.Values.CastToList(prop.GetCollectionType())))
                        {
                            break;
                        }

                        var itemSlice = slice.Slice(itemOffset);
                        implementationType = DeserializeAsAttribute.GetTargetType(target, prop, itemSlice);

                        var item = prop.CreateCollectionItem(target, implementationType);
                        if (item != null)
                        {
                            var ChildBinarySegmentAttribute = item.GetType().GetCustomAttribute<BinarySegmentAttribute>();
                            item = _Deserialize(prop.HasAttribute<NestedFileAttribute>() ? itemSlice : bytes,
                                                item,
                                                prop.HasAttribute<NestedFileAttribute>() ? [] :
                                                [(pos+itemOffset, ChildBinarySegmentAttribute?.HeaderOffset ?? 0, ChildBinarySegmentAttribute?.BodyOffset ?? 0), .. offsetsHistory],
                                                out var itemByteLength);
                            
                            items[itemOffset] = (IBinarySegment)item;

                            itemOffset += itemByteLength;
                        }
                        n++;

                        if (count >= 0 && n >= count)
                        { break; }
                    }
                    if (prop.IsAssignableTo<IBinarySegment[]>())
                    {
                        newValue = items.Values.CastToArray(prop.PropertyType.GetCollectionType()!);
                    }
                    else if (prop.IsAssignableTo<IEnumerable<IBinarySegment>>())
                    {
                        newValue = items.Values.CastToList(prop.PropertyType.GetCollectionType()!);
                    }
                    else if (prop.IsAssignableTo<IDictionary>())
                    {
                        newValue = items.CastToDict(prop.PropertyType.GetCollectionType()!);
                    }
                }
                //TODO read byte[] and similar directly because reflection on every byte is fucking slow
                else if (prop.PropertyType == typeof(byte[]))
                {
                    var array = slice.ToArray();
                    if (count>=0)
                    {
                        array = array.Take(count).ToArray();
                    }
                    newValue = array;
                }
                //primitive types
                else if (prop.IsCollection())
                {
                    var itemType = prop.GetCollectionType();

                    Dictionary<int, object> items = new();
                    int itemOffset = 0;
                    int n = 0;
                    while (itemOffset < slice.Length)
                    {
                        if (!CollectionAttribute.ShouldContinueDeserialization(target, prop, items.Values.ToList()))
                        {
                            break;
                        }

                        var item = _DeserializePrimitive(itemType, slice.Slice(itemOffset), out var itemByteLength, metadata);

                        //TODO decide if dict should be addressed by in-list offset or by pos+itemOffset
                        items[itemOffset] = item;

                        itemOffset += itemByteLength;

                        n++;

                        if (count >= 0 && n >= count)
                        { break; }
                    }

                    //materialize to match collection datatype
                    if (prop.PropertyType.IsArray)
                    {
                        newValue = items.Values.CastToArray(prop.PropertyType.GetCollectionType()!);
                    }
                    else if (prop.IsAssignableTo<IEnumerable<object>>())
                    {
                        newValue = items.Values.CastToList(prop.PropertyType.GetCollectionType()!);
                    }
                    else if (prop.IsAssignableTo<IDictionary>())
                    {
                        newValue = items.CastToDict(prop.PropertyType.GetCollectionType()!);
                    }

                }
                else
                {
                    newValue = _DeserializePrimitive(prop.PropertyType, slice, out var itemByteLength, metadata);
                }

                if (newValue != null)
                {
                    (bool isValid, object? expectedValue) = ExpectedValueAttribute.Validate(target, prop, newValue);
                    if (!isValid)
                    {
                        throw new Exception($"Invalid value for {prop.Name}, expected {expectedValue} actual {newValue}");
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

        public static List<byte> Serialize<T>(T source)
            where T : IBinarySegment
        {
            return _Serialize(source, [], [], out _);
        }

        private static List<byte> _Serialize(object source, List<(int absolute, int header, int body)> offsetsHistory, List<byte>? result, out int segmentLength)
        {
            var BinarySegmentAttribute = source.GetType().GetCustomAttribute<BinarySegmentAttribute>()!;

            if (BinarySegmentAttribute == null)
            {
                throw new Exception($"{nameof(BinarySegmentAttribute)} is required");
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
                .WithAttribute<BinaryFieldAttribute>(inherit: true)
                .Where(i => SerializeIfAttribute.If(source, i.prop))
                .OrderBy(i => OrderAttribute.GetSerializationOrder(source, i.prop))
                )
            {
                IEnumerable<byte>? fieldBytes = null;

                var metadata = source.GetFieldMetadata(prop, BinaryFieldAttribute, offsetsHistory);
                (int pos, int length, int count, bool littleEndian, int alignment, bool nullTerminated, Encoding encoding) = metadata;

                bool isNestedFile = prop.HasAttribute<NestedFileAttribute>();

                var obj = prop.GetValue(source);
                if (prop.IsAssignableTo<IBinarySegment>())
                {
                    var x = (IBinarySegment)prop.GetValue(source)!;

                    var ChildBinarySegmentAttribute = x.GetType().GetCustomAttribute<BinarySegmentAttribute>();

                    if (isNestedFile)
                    {
                        var nestedResult = _Serialize(x, [], null, out _);
                        result.Emplace(pos, nestedResult);
                    }
                    else
                    {
                        _Serialize(x, [(pos, ChildBinarySegmentAttribute?.HeaderOffset ?? 0, ChildBinarySegmentAttribute?.BodyOffset ?? 0), .. offsetsHistory], result, out _);
                    }
                }
                else if (prop.IsAssignableTo<IBinarySegment[]>() || prop.IsAssignableTo<IEnumerable<IBinarySegment>>())
                {
                    IEnumerable<IBinarySegment> items = (prop.GetValue(source) as IEnumerable<IBinarySegment>)!;

                    int childOffset = pos;

                    foreach (var x in items)
                    {
                        var ChildBinarySegmentAttribute = x.GetType().GetCustomAttribute<BinarySegmentAttribute>();

                        if (isNestedFile)
                        {
                            var nestedResult = _Serialize(x, [], null, out var itemLength);
                            result.Emplace(childOffset, nestedResult);
                            childOffset += itemLength;
                        }
                        else
                        {
                            _Serialize(x, [(childOffset, ChildBinarySegmentAttribute?.HeaderOffset ?? 0, ChildBinarySegmentAttribute?.BodyOffset ?? 0), .. offsetsHistory], result, out var itemLength);
                            childOffset += itemLength;
                        }
                    }
                }
                else if (prop.IsCollection())
                {
                    var value = prop.GetValue(source);
                    bool isDict = value is IDictionary;
                    var items = isDict
                        ? ((value as IDictionary)!.Values as IEnumerable)!
                        : (value as IEnumerable)!;

                    var e = items.GetEnumerator();

                    List<byte> b = new List<byte>();

                    while (e.MoveNext())
                    {
                        b.AddRange(_SerializePrimitive(e.Current, metadata));
                    }

                    (e as IDisposable)?.Dispose();

                    fieldBytes = b;
                }
                else
                {
                    fieldBytes = _SerializePrimitive(obj, metadata);
                }

                if (fieldBytes != null)
                {
                    result.Emplace(pos, fieldBytes);
                }
            }

            segmentLength = BinarySegmentAttribute.GetLength(source);
            return result;
        }

    }
}
