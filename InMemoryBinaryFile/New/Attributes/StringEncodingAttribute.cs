using System.Text;

namespace InMemoryBinaryFile.New.Attributes
{
    public class StringEncodingAttribute : BinaryFieldAttribute
    {
        public int CodePage { get; set; } = -1;

        public Encoding? Encoding => (CodePage >= 0 && CodePage <= 65535) ? Encoding.GetEncoding(CodePage) : null;
        public string? EncodingFunc { get; set; }

        public Encoding GetEncoding(object obj, string FieldName) => ReflectionsHelper.GetDynamicValue(() => Encoding, (x) => x != null, EncodingFunc ?? $"{FieldName}Encoding", obj);
    }
}
