namespace InMemoryBinaryFile.New.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class BinarySegmentAttribute : Attribute
    {
        public int HeaderOffset { get; set; }
        public int BodyOffset { get; set; }
    }
}
