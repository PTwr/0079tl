using InMemoryBinaryFile.New.Attributes;
using System.Reflection;

namespace InMemoryBinaryFile.New.Serialization
{
    internal static class SerializationHelpers
    {
        internal static IEnumerable<(TAttrib attr, PropertyInfo prop)> WithAttribute<TAttrib>(this IEnumerable<PropertyInfo> props, bool inherit = false)
            where TAttrib : Attribute
        {
            foreach (var prop in props)
            {
                var attrib = prop.GetCustomAttribute<TAttrib>(inherit);
                if (attrib != null)
                {
                    yield return (attrib, prop);
                }
            }
        }

        internal static IEnumerable<PropertyInfo> Properties(this object obj, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            return obj.GetType().GetProperties(bindingFlags);
        }

        internal static (int pos, int length, int count) GetFieldMetadata(this object obj, PropertyInfo prop, BinaryFieldAttribute attrib, List<(int absolute, int header, int body)> offsetsHistory)
        {
            var pos = attrib.GetOffset(obj, prop);
            var length = attrib.GetLength(obj, prop);
            var count = attrib.GetCount(obj, prop);

            var offsetScope = offsetsHistory.Last();
            switch (attrib.OffsetScope)
            {
                case OffsetScope.Absolute:
                    offsetScope = offsetsHistory.Last();
                    break;
                case OffsetScope.Segment:
                    offsetScope = offsetsHistory[0];
                    break;
                case OffsetScope.Parent:
                    offsetScope = offsetsHistory[1];
                    break;
                case OffsetScope.GrandParent:
                    offsetScope = offsetsHistory[2];
                    break;
                default:
                    break;
            }

            pos += offsetScope.absolute;
            switch (attrib.OffsetZone)
            {
                case OffsetZone.Absolute:
                    break;
                case OffsetZone.Header:
                    pos += offsetScope.header;
                    break;
                case OffsetZone.Body:
                    pos += offsetScope.body;
                    break;
                default:
                    break;
            }

            return (pos, length, count);
        }

        internal static Span<byte> Segment(this Span<byte> bytes, int pos, int l)
        {
            return (l >= 0) ? bytes.Slice(pos, l) : bytes.Slice(pos);
        }
    }
}
