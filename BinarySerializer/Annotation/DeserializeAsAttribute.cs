using BinarySerializer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BinarySerializer.Annotation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public abstract class ConditionalSerializationAttribute : Attribute
    {
        public string? IfFunc { get; set; }
        public virtual bool GetIf(object obj, string FieldName) => ReflectionsHelper.GetDynamicValue<bool>(IfFunc ?? $"{FieldName}If", obj, true);
    }

    public class DeserializeIfAttribute : ConditionalSerializationAttribute
    {
        public int[]? IfStartsWithPattern { get; set; } = null;

        public override bool GetIf(object obj, string FieldName) =>
            base.GetIf(obj, FieldName) &&
            ReflectionsHelper.GetDynamicValue<bool>(IfFunc ?? $"{FieldName}DeserializeIf", obj, true);

        public bool GetIf(object obj, string FieldName, Span<byte> data)
        {
            if (!GetIf(obj, FieldName))
            {
                return false;
            }

            if (IfStartsWithPattern != null && !data.StartsWith(IfStartsWithPattern))
            {
                return false;
            }

            return true;
        }
        public static bool If(object obj, PropertyInfo propertyInfo, Span<byte> data)
        {
            //CS9108 - compiler can't (safely) capture stackallock Span for lambda due to risk of deferred execution
            //return obj.CheckAttribute<DeserializeIfAttribute, bool>(propertyInfo, (a) => a.GetIf(obj, propertyInfo.Name, data), true);

            var attribs = propertyInfo
                .GetCustomAttributes<DeserializeIfAttribute>()
                .OrderBy(i => (i as IOrderedAttribute)?.Order ?? 0);

            var hardcoded = ReflectionsHelper.GetDynamicValue<bool>($"{propertyInfo.Name}If", obj, true)
                && ReflectionsHelper.GetDynamicValue<bool>($"{propertyInfo.Name}DeserializeIf", obj, true);

            if (!hardcoded)
            {
                return false;
            }

            foreach (var attrib in attribs)
            {
                if (attrib.GetIf(obj, propertyInfo.Name, data))
                {
                    return true;
                }
            }

            return hardcoded;
        }
    }
    public class SerializeIfAttribute : ConditionalSerializationAttribute
    {
        public override bool GetIf(object obj, string FieldName) =>
            base.GetIf(obj, FieldName) &&
            ReflectionsHelper.GetDynamicValue<bool>(IfFunc ?? $"{FieldName}SerializeIf", obj, true);

        public bool If(object obj)
        {
            if (string.IsNullOrWhiteSpace(IfFunc) || !ReflectionsHelper.GetDynamicValue<bool>(IfFunc, obj, false))
            {
                return false;
            }

            return true;
        }
        public static bool If(object obj, PropertyInfo propertyInfo)
        {
            var hardcoded = ReflectionsHelper.GetDynamicValue<bool>($"{propertyInfo.Name}If", obj, true)
                && ReflectionsHelper.GetDynamicValue<bool>($"{propertyInfo.Name}SerializeIf", obj, true);
            return hardcoded &&
                obj.CheckAttribute<SerializeIfAttribute, bool>(propertyInfo, (a) => a.GetIf(obj, propertyInfo.Name), true);
        }
    }

    public interface IOrderedAttribute
    {
        public int Order { get; set; }
    }
    public class DeserializeAsAttribute : DeserializeIfAttribute, IOrderedAttribute
    {
        public int Order { get; set; }
        public Type Type { get; set; }

        public static Type GetTargetType(object obj, PropertyInfo propertyInfo, Span<byte> data)
        {
            var attribs = propertyInfo
                .GetCustomAttributes<DeserializeAsAttribute>()
                .OrderBy(i => i.Order);

            foreach (var attrib in attribs)
            {
                if (attrib.GetIf(obj, propertyInfo.Name, data))
                {
                    return attrib.Type;
                }
            }

            if (propertyInfo.PropertyType == typeof(IBinarySegment))
            {
                return typeof(RawBinarySegment);
            }

            return propertyInfo.PropertyType;
        }
    }

    public class DeserializeAsAttribute<T> : DeserializeAsAttribute
        where T : IBinarySegment
    {
        public DeserializeAsAttribute()
        {
            Type = typeof(T);
        }

        public DeserializeAsAttribute(int pattern) : this()
        {
            //DISGUSTING!
            this.IfStartsWithPattern = pattern
                .GetBigEndianBytes()
                .Select(i => (int)i)
                .ToArray();
        }
    }
}
