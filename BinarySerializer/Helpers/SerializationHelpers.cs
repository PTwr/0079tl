using BinarySerializer.Annotation;
using System.Reflection;
using System.Text;

namespace BinarySerializer.Helpers
{
    internal static class SerializationHelpers
    {
        public static bool HasAttribute<TAttribute>(this PropertyInfo propertyInfo)
            where TAttribute : Attribute
        {
            return propertyInfo
                .GetCustomAttributes<TAttribute>()
                .Any();
        }

        public static TResult? CheckAttribute<TAttribute, TResult>(this object obj, PropertyInfo propertyInfo, Func<TAttribute, TResult> logic, TResult? defaultResult = default)
            where TAttribute : Attribute
        {
            var attribs = propertyInfo
                .GetCustomAttributes<TAttribute>();

            foreach (var attrib in attribs)
            {
                return logic(attrib);
            }

            return defaultResult;
        }

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

        internal static (int pos, int length, int count, bool littleEndian, int alignment, bool nullTerminated, Encoding encoding) GetFieldMetadata(this object obj, PropertyInfo prop, BinaryFieldAttribute attrib, List<(int absolute, int header, int body)> offsetsHistory)
        {
            var nullTerminated = prop.HasAttribute<NullTerminatedAttribute>();
            var encoding = StringEncodingAttribute.GetEncoding(obj, prop);

            var pos = attrib.GetOffset(obj, prop);
            var length = attrib.GetLength(obj, prop);
            var count = CollectionAttribute.GetExpectedCount(obj, prop);

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
                    //lookup further into history than predefined anchors
                    offsetScope = offsetsHistory[(int)attrib.OffsetScope];
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

            return (pos, length, count, prop.HasAttribute<LittleEndianAttribute>(), attrib.Alignment, nullTerminated, encoding);
        }

        internal static Span<byte> Segment(this Span<byte> bytes, int pos, int l)
        {
            return (l >= 0) ? bytes.Slice(pos, l) : bytes.Slice(pos);
        }
    }
}
