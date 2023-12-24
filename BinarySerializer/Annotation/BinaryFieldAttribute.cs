using BinarySerializer.Helpers;
using System.Collections;
using System.Reflection;

namespace BinarySerializer.Annotation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class OrderAttribute : Attribute
    {
        public OrderAttribute(int order)
        {
            this.Order = order;
            this.SerializationOrder = null;
        }
        public OrderAttribute(int order, int serializationOrder)
        {
            this.Order = order;
            this.SerializationOrder = serializationOrder;
        }

        /// <summary>
        /// Specifies order of (De)Serialization
        /// </summary>
        public int Order { get; set; }
        /// <summary>
        /// Overrides Order for Serialization
        /// </summary>
        public int? SerializationOrder { get; set; }

        public static int GetSerializationOrder(object obj, PropertyInfo propertyInfo)
        {
            return obj.CheckAttribute<OrderAttribute, int>(propertyInfo, (a) => a.SerializationOrder ?? a.Order, 0);
        }
        public static int GetDeserializationOrder(object obj, PropertyInfo propertyInfo)
        {
            return obj.CheckAttribute<OrderAttribute, int>(propertyInfo, (a) => a.Order, 0);
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class CollectionAttribute : Attribute
    {
        /// <summary>
        /// Limits item count when deserializing this property
        /// </summary>
        public int Count { get; set; } = -1;
        /// <summary>
        /// Function to calculate Count in runtime
        /// Defaults to int {FieldName}Length() {...}
        /// </summary>
        public string? CountFunc { get; set; }

        /// <summary>
        /// Function to determine if collection (de)serialization should continue
        /// Defaults to bool {FieldName}Continue(collection) {...}
        /// If unspecified, serialization will continue for whole colelction
        /// and deserialization until byte stream specified by Length runs out.
        /// </summary>
        public string? ContinueFunc { get; set; }

        int GetCount(object obj, string FieldName) => ReflectionsHelper.GetDynamicValue(() => Count, (x) => x >= 0, CountFunc ?? $"{FieldName}Count", obj, -1);
        int GetCount(object obj, PropertyInfo property) => GetCount(obj, property.Name);

        bool GetContinue(object obj, string FieldName, IEnumerable collection) => ReflectionsHelper.GetDynamicValue<bool>(ContinueFunc ?? $"{FieldName}Continue", obj, true, collection);
        bool GetContinue(object obj, PropertyInfo property, IEnumerable collection) => GetContinue(obj, property.Name, collection);

        public static bool ShouldContinueDeserialization(object obj, PropertyInfo propertyInfo, IEnumerable collection)
        {
            return obj.CheckAttribute<CollectionAttribute, bool>(propertyInfo, (a) => a.GetContinue(obj, propertyInfo, collection), true);
        }

        public static int GetExpectedCount(object obj, PropertyInfo propertyInfo)
        {
            return obj.CheckAttribute<CollectionAttribute, int>(propertyInfo, (a) => a.GetCount(obj, propertyInfo), -1);
        }
    }

    /// <summary>
    /// Marks field as being written in LittleEndian, rather than usual BigEndian
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class LittleEndianAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class ExpectedValueAttribute : Attribute
    {
        protected object? ExpectedValue { get; set; }
        protected Type ValueType { get; set; }

        public ExpectedValueAttribute(object? value, Type valueType)
        {
            this.ExpectedValue = value;
            this.ValueType = valueType;
        }

        private static (bool isValid, object? expectedValue) CompareObjects(PropertyInfo propertyInfo, object? expectedValue, object? newValue)
        {
            //xor nullchecks to not overcomplicate later casting :)
            if (newValue == null ^ expectedValue == null)
            {
                return (false, expectedValue);
            }

            //fuck nullchecks
            if (newValue == null && expectedValue == null)
            {
                return (true, expectedValue);
            }

            //from now now both values are guaranted to be not null... unless some evil multithreading happens :D

            //try to skip type conversion bullshit if possible
            //Equals on primitives is not virtual so int.Equals behaves differently from ((object)int).Equals
            //which makes ExpectedValueAttribute(123) on short/byte properties annoying
            if (expectedValue!.Equals(newValue))
            {
                123.Equals(123);
                return (true, expectedValue);
            }

            //try to cast both sides to propertytype to "fix" object.Equals

            if (!expectedValue.TryCast(propertyInfo.PropertyType, out var x))
            {
                throw new Exception($"Expected value of '{expectedValue}' can't be cast to '{propertyInfo.PropertyType.FullName}'");
            }

            if (!newValue!.TryCast(propertyInfo.PropertyType, out var y))
            {
                throw new Exception($"Actual value of '{newValue}' can't be cast to '{propertyInfo.PropertyType.FullName}'");
            }

            //Equals should have no type mismatch issues now
            if (x!.Equals(y))
            {
                return (true, expectedValue);
            }

            return (false, expectedValue);
        }
        public static (bool isValid, object? expectedValue) Validate(object obj, PropertyInfo propertyInfo, object? newValue)
        {
            return obj.CheckAttribute<ExpectedValueAttribute, (bool isValid, object? expectedValue)>(propertyInfo, (a) =>
            {
                if (a.ExpectedValue is IEnumerable && newValue is IEnumerable)
                {
                    //based on https://stackoverflow.com/a/59934344/3147740
                    var expected = ((IEnumerable)a.ExpectedValue).GetEnumerator();
                    var actual = ((IEnumerable)newValue).GetEnumerator();

                    try
                    {
                        //until end of expected
                        while (expected.MoveNext())
                        {
                            //if actual is shorter than expected
                            if (!actual.MoveNext())
                            {
                                return (false, a.ExpectedValue);
                            }

                            //perform typecasting bullshit on each item pair
                            var test = CompareObjects(propertyInfo, expected.Current, actual.Current);

                            //stop on first mismatch
                            if (!test.isValid)
                            {
                                return (false, a.ExpectedValue);
                            }
                        }

                        //if actual still has items left, then its too long
                        return (!actual.MoveNext(), a.ExpectedValue);
                    }
                    finally
                    {
                        (expected as IDisposable)?.Dispose();
                        (actual as IDisposable)?.Dispose();
                    }
                }
                else
                {
                    return CompareObjects(propertyInfo, a.ExpectedValue, newValue);
                }
            }, (true, null));
        }
    }

    public sealed class ExpectedValueAttribute<TValue> : ExpectedValueAttribute
    {
        public ExpectedValueAttribute(TValue value)
            : base(value, typeof(TValue))
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class BinaryFieldAttribute : Attribute
    {
        public int Alignment { get; set; } = 1;

        public bool SeparateScope { get; set; } = false;

        /// <summary>
        /// Limits length of byte buffer used to deserialize this property
        /// </summary>
        public int Length { get; set; } = -1;
        /// <summary>
        /// Function to calculate Length in runtime
        /// Defaults to int {FieldName}Length() {...}
        /// </summary>
        public string? LengthFunc { get; set; }

        /// <summary>
        /// Position in source stream. If less than 0, PositionFunc will be used
        /// </summary>
        public int Offset { get; set; } = -1;
        /// <summary>
        /// Function to calculate Offset in runtime
        /// Defaults to int {FieldName}Position() {...}
        /// </summary>
        public string? OffsetFunc { get; set; }

        /// <summary>
        /// Controls Position Offset automatic calculation in given OffsetScope
        /// </summary>
        public OffsetZone OffsetZone { get; set; } = OffsetZone.Absolute;
        /// <summary>
        /// Controls Position Offset scope selection for automatic calculation
        /// </summary>
        public OffsetScope OffsetScope { get; set; } = OffsetScope.Segment;

        public int GetOffset(object obj, string FieldName) => ReflectionsHelper.GetDynamicValue(() => Offset, (x) => x >= 0, OffsetFunc ?? $"{FieldName}Offset", obj, -1);
        public int GetOffset(object obj, PropertyInfo property) => GetOffset(obj, property.Name);

        public int GetLength(object obj, string FieldName) => ReflectionsHelper.GetDynamicValue(() => Length, (x) => x >= 0, LengthFunc ?? $"{FieldName}Length", obj, -1);
        public int GetLength(object obj, PropertyInfo property) => GetLength(obj, property.Name);
    }
}
