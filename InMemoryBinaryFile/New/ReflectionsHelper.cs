using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryBinaryFile.New
{
    public static class ReflectionsHelper
    {
        //https://stackoverflow.com/a/4102028
        public static bool CanChangeType(object value, Type conversionType)
        {
            if (conversionType == null)
            {
                return false;
            }

            if (value == null)
            {
                return false;
            }

            IConvertible? convertible = value as IConvertible;

            if (convertible == null)
            {
                return false;
            }

            return true;
        }

        public static T GetDynamicValue<T>(Func<T?> staticValue, Func<T, bool> staticValidator, string? dynamicMemberName, object obj)
        {
            var result = staticValue();
            if (staticValidator(result))
            {
                return result;
            }

            if (dynamicMemberName != null)
            {
                object? reflectionResult = null;

                var func = obj.GetType()
                    .GetMethod(dynamicMemberName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (func != null
                    &&
                    func.GetParameters().Length == 0)
                {
                    reflectionResult = func.Invoke(obj, null);
                }

                var prop = obj.GetType()
                    .GetProperty(dynamicMemberName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (prop != null)
                {
                    reflectionResult = prop.GetValue(obj);
                }

                if (reflectionResult is T)
                {
                    return (T)reflectionResult;
                }
            }

            return result;
        }
    }
}
