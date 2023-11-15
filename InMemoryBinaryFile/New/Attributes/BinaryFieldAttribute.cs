using System.Reflection;

namespace InMemoryBinaryFile.New.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class BinaryFieldAttribute : Attribute
    {
        /// <summary>
        /// Limits item count when deserializing this property
        /// </summary>
        public int Count { get; set; } = -1;
        /// <summary>
        /// Function to calculate Count in runtime
        /// Defaults to int FieldNameLength() {...}
        /// </summary>
        public string? CountFunc { get; set; }

        /// <summary>
        /// Limits length of byte buffer used to deserialize this property
        /// </summary>
        public int Length { get; set; } = -1;
        /// <summary>
        /// Function to calculate Length in runtime
        /// Defaults to int FieldNameLength() {...}
        /// </summary>
        public string? LengthFunc { get; set; }

        /// <summary>
        /// Switches from logical BigEndian to illogical LittleEndian Intel forced on us in memory
        /// </summary>
        public bool LittleEndian { get; set; }

        /// <summary>
        /// for when Field Position requires other field to be parsed first
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Position in source stream. Used less than 0, PositionFunc will be used
        /// </summary>
        public int Position { get; set; } = -1;
        /// <summary>
        /// Controls Position Offset automatic calculation in scope of segment
        /// </summary>
        public FieldOffset FieldOffset { get; set; } = FieldOffset.Absolute;
        /// <summary>
        /// Controls Position Offset scope selection for automatic calculation
        /// </summary>
        public SegmentOffset SegmentOffset { get; set; } = SegmentOffset.Absolute;
        /// <summary>
        /// Function to calculate Position in runtime
        /// Defaults to int FieldNamePosition() {...}
        /// </summary>
        public string? PositionFunc { get; set; }

        public object? ExpectedValue { get; set; } = null;

        public int GetPosition(object obj, string FieldName) => ReflectionsHelper.GetDynamicValue(() => Position, (x) => x >= 0, PositionFunc ?? $"{FieldName}Position", obj);
        public int GetLength(object obj, string FieldName) => ReflectionsHelper.GetDynamicValue(() => Length, (x) => x >= 0, LengthFunc ?? $"{FieldName}Length", obj);
        public int GetCount(object obj, string FieldName) => ReflectionsHelper.GetDynamicValue(() => Count, (x) => x >= 0, CountFunc ?? $"{FieldName}Count", obj);
    }
}
