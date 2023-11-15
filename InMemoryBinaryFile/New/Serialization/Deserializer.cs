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
        public static T Deserialize<T>(Span<byte> bytes, T target)
            where T : IBinaryFile
        {
            var BinaryFileAttribute = target.GetType().GetCustomAttribute<BinarySegmentAttribute>();

            foreach (var prop in target.GetType()
                .GetProperties()
                .Where(i => i.GetCustomAttribute<BinaryFieldAttribute>(true) != null)
                .OrderBy(i => i.GetCustomAttribute<BinaryFieldAttribute>(true)?.Order)
                )
            {
                object? newValue = null;

                var BinaryFieldAttribute = prop.GetCustomAttribute<BinaryFieldAttribute>();
                if (BinaryFieldAttribute == null)
                {
                    continue;
                }
                var pos = BinaryFieldAttribute.GetPosition(target, prop.Name);
                var l = BinaryFieldAttribute.GetLength(target, prop.Name);
                var count = BinaryFieldAttribute.GetCount(target, prop.Name);

                if (BinaryFileAttribute != null)
                {
                    switch (BinaryFieldAttribute.FieldOffset)
                    {
                        case FieldOffset.Absolute:
                            break;
                        case FieldOffset.Header:
                            pos += BinaryFileAttribute.HeaderOffset;
                            break;
                        case FieldOffset.Body:
                            pos += BinaryFileAttribute.BodyOffset;
                            break;
                        default:
                            break;
                    }
                }

                var slice = (l >= 0) ? bytes.Slice(pos, l) : bytes.Slice(pos);

                var StringEncodingAttribute = prop.GetCustomAttribute<StringEncodingAttribute>();
                var NullTerminatedStringAttribute = prop.GetCustomAttribute<NullTerminatedStringAttribute>();
                var FixedLengthStringAttribute = prop.GetCustomAttribute<FixedLengthStringAttribute>();

                var encoding = StringEncodingAttribute?.GetEncoding(target, prop.Name) ?? Encoding.ASCII;

                if (prop.PropertyType == typeof(byte))
                {
                    newValue = bytes[pos];
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
                        .ToDecodedNullTerminatedStrings(encoding, count)
                        .ToArray();
                }
                else if (prop.PropertyType == typeof(List<string>) && NullTerminatedStringAttribute != null)
                {
                    newValue = slice
                        .ToDecodedNullTerminatedStrings(encoding, count);
                }
                else if (prop.PropertyType == typeof(Dictionary<int, string>) && NullTerminatedStringAttribute != null)
                {
                    newValue = slice
                        .ToDecodedNullTerminatedStringDict(encoding, count);
                }

                if (newValue != null)
                {
                    if (BinaryFieldAttribute.ExpectedValue != null)
                    {
                        if ((
                                BinaryFieldAttribute.ExpectedValue is string 
                                && 
                                (newValue.ToString() != BinaryFieldAttribute.ExpectedValue.ToString())
                            )
                            || 
                            (newValue != BinaryFieldAttribute.ExpectedValue))
                        {

                        }
                        else if (newValue != BinaryFieldAttribute.ExpectedValue)
                        {
                            throw new Exception($"Invalid value for {prop.Name}, expected {BinaryFieldAttribute.ExpectedValue} actual {newValue}");
                        }
                    }

                    prop.SetValue(target, newValue);
                }
            }

            return target;
        }

        public static T Deserialize<T>(Span<byte> bytes)
            where T : IBinaryFile, new()
        {
            var baseAttribType = typeof(BinaryFieldAttribute);

            T t = new T();

            return Deserialize(bytes, t);
        }
    }
}
