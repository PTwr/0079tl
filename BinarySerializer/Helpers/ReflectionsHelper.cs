using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BinarySerializer.Helpers
{
    public static class ReflectionsHelper
    {
        public static bool TryCast(this object obj, Type type, out object? value)
        {
            try
            {
                value = Convert.ChangeType(obj, type);
                return true;
            }
            catch { }

            value = null;
            return false;
        }

        //https://stackoverflow.com/a/55852845
        public static T CastTo<T>(this object o) => (T)o;
        //https://stackoverflow.com/a/55852845
        public static object? CastToReflected(this object o, Type type)
        {
            var methodInfo = typeof(ReflectionsHelper).GetMethod(nameof(CastTo), BindingFlags.Static | BindingFlags.Public);
            var genericArguments = new[] { type };
            var genericMethodInfo = methodInfo?.MakeGenericMethod(genericArguments);
            return genericMethodInfo?.Invoke(null, new[] { o });
        }

        public static object CastToArray(this IEnumerable<object> source, Type type)
        {
            var arr = Array.CreateInstance(type, source.Count());

            int n = 0;
            foreach (var item in source)
            {
                arr.SetValue(item, n);
                n++;
            }

            return arr;
        }
        public static object CastToList(this IEnumerable<object> source, Type type)
        {
            object list = typeof(List<>)
                .MakeGenericType([type])!
                .GetConstructor([typeof(int)])!
                .Invoke([source.Count()]);

            var addMethodInfo = list.GetType()
                .GetMethod(nameof(List<int>.Add));

            int n = 0;
            foreach (var item in source)
            {
                addMethodInfo.Invoke(list, [item]);
                n++;
            }

            return list;
        }
        public static object CastToDict<T>(this IDictionary<int, T> source, Type type)
        {
            object dict = typeof(Dictionary<,>)
                .MakeGenericType([typeof(int), type])!
                .GetConstructor([typeof(int)])!
                .Invoke([source.Count()]);

            var addMethodInfo = dict.GetType()
                .GetMethod(nameof(Dictionary<int, int>.Add));

            int n = 0;
            foreach (var item in source)
            {
                addMethodInfo.Invoke(dict, [item.Key, item.Value]);
                n++;
            }

            return dict;
        }

        public static object Cast(this IEnumerable source, Type type)
        {
            var mi = typeof(System.Linq.Enumerable)
                .GetMethod(nameof(System.Linq.Enumerable.Cast), BindingFlags.Static | BindingFlags.Public);
            var gmi = mi?.MakeGenericMethod([type]);

            object x = gmi.Invoke(null, [source]);

            return x;
        }
        public static object? CreatePropertyObject(this PropertyInfo prop, object parentObject, Type? implementationType = null)
        {
            var type = implementationType ?? prop.PropertyType;

            return
                //manualy initialized object
                prop.GetValue(parentObject)
                ??
                //constructor accepting parent object
                type.GetConstructor([parentObject.GetType()])?.Invoke([parentObject])
                ??
                //parameterless constructor
                type.GetConstructor(Type.EmptyTypes)?.Invoke(null);
        }
        public static object? CreateCollectionItem(this PropertyInfo prop, object parentObject, Type? implementationType = null)
        {
            var type = implementationType ?? prop.PropertyType;
            type = type.IsCollection() ? type.GetCollectionType() : type;

            if (type == null)
            {
                //not recognized as collection
                return null;
            }

            return
                //constructor accepting parent object
                type.GetConstructor([parentObject.GetType()])?.Invoke([parentObject])
                ??
                //parameterless constructor
                type.GetConstructor(Type.EmptyTypes)?.Invoke(null);
        }
        public static bool IsAssignableTo<TInterface>(this Type type)
        {
            return type.IsAssignableTo(typeof(TInterface));
        }
        public static bool IsAssignableTo<TInterface>(this PropertyInfo prop)
        {
            return prop.PropertyType.IsAssignableTo(typeof(TInterface));
        }
        public static Type? GetCollectionType(this PropertyInfo prop)
        {
            return prop.PropertyType.GetCollectionType();
        }
        public static Type? GetCollectionType(this Type type)
        {
            return type.GetGenericArguments().LastOrDefault()
                ?? type.GetElementType();
        }
        public static bool IsCollection(this PropertyInfo prop)
        {
            return prop.PropertyType.IsCollection();
        }
        public static bool IsCollection(this Type prop)
        {
            return prop.IsArray
                || prop.IsAssignableTo<IEnumerable<object>>()
                //TODO check for key type? only int is supported currently
                || prop.IsAssignableTo<IDictionary>();
        }

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

        public static T? GetDynamicValue<T>(Func<T?> staticValue, Func<T?, bool> staticValidator, string? dynamicMemberName, object obj, T? defaultValue = default)
        {
            var result = staticValue();
            if (staticValidator(result))
            {
                return result;
            }

            return GetDynamicValue<T>(dynamicMemberName, obj, defaultValue);
        }

        public static T? GetDynamicValue<T>(string? dynamicMemberName, object obj, T? defaultValue, params object[] arguments)
        {
            T? result = defaultValue;

            if (dynamicMemberName != null)
            {
                object? reflectionResult = null;

                var func = obj.GetType()
                    .GetMethod(dynamicMemberName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (func != null
                    &&
                    func.GetParameters().Length == (arguments?.Length ?? 0))
                {
                    reflectionResult = func.Invoke(obj, arguments);
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
