namespace InMemoryBinaryFile.New.Attributes
{
    public class NullTerminatedStringAttribute : StringEncodingAttribute
    {
        public int Alignment { get; set; } = 1;
    }
}
