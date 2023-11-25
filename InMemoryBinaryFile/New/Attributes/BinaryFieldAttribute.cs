using InMemoryBinaryFile.New.Serialization;
using System.Reflection;

namespace InMemoryBinaryFile.New.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class BinaryFieldAttribute : Attribute
    {
        public string ContinueFunc { get; set; }

        public bool Serialize { get; set; } = true;
        public string? SerializeIfFunc { get; set; }

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
        /// for when Field has to be serialized in different order than deserialized
        /// </summary>
        public int? SerializationOrder { get; set; }

        /// <summary>
        /// Position in source stream. Used less than 0, PositionFunc will be used
        /// </summary>
        public int Offset { get; set; } = -1;
        /// <summary>
        /// Function to calculate Offset in runtime
        /// Defaults to int FieldNamePosition() {...}
        /// </summary>
        public string? OffsetFunc { get; set; }

        /// <summary>
        /// Controls Position Offset automatic calculation in given OffsetScope
        /// </summary>
        public OffsetZone OffsetZone { get; set; } = OffsetZone.Absolute;
        /// <summary>
        /// Controls Position Offset scope selection for automatic calculation
        /// </summary>
        public OffsetScope OffsetScope { get; set; } = OffsetScope.Absolute;

        public object? ExpectedValue { get; set; } = null;

        /// <summary>
        /// Function to determine when field should be serialized
        /// defaults to bool FieldNameIf() {...}
        /// </summary>
        public string? IfFunc { get; set; }

        public int GetOffset(object obj, string FieldName) => ReflectionsHelper.GetDynamicValue(() => Offset, (x) => x >= 0, OffsetFunc ?? $"{FieldName}Offset", obj, -1);
        public int GetOffset(object obj, PropertyInfo property) => GetOffset(obj, property.Name);

        public int GetLength(object obj, string FieldName) => ReflectionsHelper.GetDynamicValue(() => Length, (x) => x >= 0, LengthFunc ?? $"{FieldName}Length", obj, -1);
        public int GetLength(object obj, PropertyInfo property) => GetLength(obj, property.Name);

        public int GetCount(object obj, string FieldName) => ReflectionsHelper.GetDynamicValue(() => Count, (x) => x >= 0, CountFunc ?? $"{FieldName}Count", obj, -1);
        public int GetCount(object obj, PropertyInfo property) => GetCount(obj, property.Name);

        public bool GetIf(object obj, string FieldName) => ReflectionsHelper.GetDynamicValue<bool>(IfFunc ?? $"{FieldName}If", obj, true);
        public bool GetIf(object obj, PropertyInfo property) => GetIf(obj, property.Name);

        public bool GetSerializeIf(object obj, string FieldName) => ReflectionsHelper.GetDynamicValue<bool>(SerializeIfFunc ?? $"{FieldName}SerializeIf", obj, true);
        public bool GetSerializeIf(object obj, PropertyInfo property) => GetSerializeIf(obj, property.Name);

        public bool GetContinue(object obj, string FieldName, object collection) => ReflectionsHelper.GetDynamicValue<bool>(ContinueFunc ?? $"{FieldName}Continue", obj, true, collection);
        public bool GetContinue(object obj, PropertyInfo property, object collection) => GetContinue(obj, property.Name, collection);
    }
}
