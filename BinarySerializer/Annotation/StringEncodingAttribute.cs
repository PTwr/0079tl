using BinarySerializer.Helpers;
using System.Reflection;
using System.Text;

namespace BinarySerializer.Annotation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class StringEncodingAttribute : Attribute
    {
        public int CodePage { get; set; } = Encoding.ASCII.CodePage;

        public Encoding? Encoding => (CodePage >= 0 && CodePage <= 65535) ? Encoding.GetEncoding(CodePage) : null;
        public string? EncodingFunc { get; set; }

        public Encoding GetEncoding(object obj, string FieldName) => ReflectionsHelper.GetDynamicValue(() => Encoding, (x) => x != null, EncodingFunc ?? $"{FieldName}Encoding", obj) ?? Encoding.ASCII;

        public static Encoding GetEncoding(object obj, PropertyInfo propertyInfo)
        {
            return obj.CheckAttribute<StringEncodingAttribute, Encoding>(propertyInfo, (a) => a.GetEncoding(obj, propertyInfo.Name), Encoding.ASCII)!;
        }
    }
}
