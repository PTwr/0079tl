
using BinarySerializer.Helpers;

namespace BinarySerializer.Annotation
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class BinarySegmentAttribute : Attribute
    {
        public int HeaderOffset { get; set; }
        public int BodyOffset { get; set; }

        /// <summary>
        /// Limits length of byte buffer used to deserialize this object
        /// </summary>
        public int Length { get; set; } = -1;
        /// <summary>
        /// Function to calculate Length in runtime
        /// Defaults to int SegmentLength() {...}
        /// </summary>
        public string? LengthFunc { get; set; }

        public int GetLength(object obj) => ReflectionsHelper.GetDynamicValue(() => Length, (x) => x >= 0, LengthFunc ?? $"SegmentLength", obj, -1);
    }
}
