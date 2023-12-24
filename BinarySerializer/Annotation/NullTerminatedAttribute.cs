
namespace BinarySerializer.Annotation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class NullTerminatedAttribute : Attribute
    {
    }
}
