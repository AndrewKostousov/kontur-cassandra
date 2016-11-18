using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

using GrEmit;

namespace SKBKontur.Catalogue.Objects.ValueExtracting
{
    // todo утащить в отдельный проект
    public static class ObjectValueExtractor
    {
        public static object Extract<T>(T obj, string path) where T : class
        {
            return Extract(typeof(T), obj, path);
        }

        public static object Extract(Type type, object obj, string path)
        {
            var extractor = GetExtractorFunction(type, path);
            return extractor(obj);
        }

        public static Type GetPropertyType(Type type, string path)
        {
            foreach(var member in path.Split('.'))
            {
                if(type.IsValueType)
                    throw new NotSupportedException("Value types are not supported");
                var property = type.GetProperty(member, BindingFlags.Public | BindingFlags.Instance);
                if(property == null)
                    throw new PropertyNotFoundByValueExtractorException(type, path, member);
                type = property.PropertyType;
            }
            return type;
        }

        public static Type GetEnumerableParameter(Type type)
        {
            var enumerableInterface = type.GetInterface("System.Collections.Generic.IEnumerable`1");
            if(enumerableInterface == null)
                throw new ArgumentException("Type does not implement IEnumerable interface");
            return enumerableInterface.GetGenericArguments().Single();
        }

        public static IObjectPropertyValueExtractor GetExtractor(Type type, string path)
        {
            return new CustomObjectPropertyValueExtractor(GetExtractorFunction(type, path), path);
        }

        private static bool ContainsIndexes(string path, out string generalizedPath, out int[] indexes)
        {
            var containsIndexes = false;
            for(var i = 0; i < path.Length; ++i)
            {
                if(char.IsDigit(path[i]) && (i == 0 || path[i - 1] == '[' || path[i - 1] == '.'))
                {
                    containsIndexes = true;
                    break;
                }
            }
            if(!containsIndexes)
            {
                generalizedPath = path;
                indexes = null;
                return false;
            }
            var list = new List<int>();
            var stringBuilder = new StringBuilder();
            var parts = path.Split(new[] {'.', '[', ']'}, StringSplitOptions.RemoveEmptyEntries);
            for(var i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                if(!char.IsDigit(part[0]))
                    stringBuilder.Append(part);
                else
                {
                    list.Add(int.Parse(part));
                    stringBuilder.Append("@");
                }
                if(i < parts.Length - 1)
                    stringBuilder.Append(".");
            }
            generalizedPath = stringBuilder.ToString();
            indexes = list.ToArray();
            return true;
        }

        private static Func<object, object> GetExtractorFunction(Type type, string path)
        {
            if(string.IsNullOrEmpty(path))
                return obj => obj;
            string generalizedPath;
            int[] indexes;
            var containsIndexes = ContainsIndexes(path, out generalizedPath, out indexes);
            var key = type.FullName + "@" + generalizedPath;
            var extractor = (Delegate)extractors[key];
            if(extractor == null)
            {
                lock(extractorsLock)
                {
                    extractor = (Delegate)extractors[key];
                    if(extractor == null)
                    {
                        extractor = BuildExtractor(type, generalizedPath, containsIndexes);
                        extractors[key] = extractor;
                    }
                }
            }
            if(!containsIndexes)
                return (Func<object, object>)extractor;
            var func = (Func<object, int[], object>)extractor;
            return o => func(o, indexes);
        }

        private static Delegate BuildExtractor(Type type, string path, bool containsIndexes)
        {
            var method = new DynamicMethod(Guid.NewGuid().ToString(), typeof(object), containsIndexes ? new[] {typeof(object), typeof(int[])} : new[] {typeof(object)}, typeof(ObjectValueExtractor), true);
            var il = new GroboIL(method);
            var returnDefaultLabel = il.DefineLabel("returnDefaultValue");
            var index = containsIndexes ? il.DeclareLocal(typeof(int)) : null;
            il.Ldarg(0); // stack: [obj]
            il.Castclass(type); // stack: [(type)obj]
            var i = 0;
            foreach(var member in path.Split('.'))
            {
                if(type.IsValueType)
                    throw new NotSupportedException("Value types are not supported");
                il.Dup();
                il.Brfalse(returnDefaultLabel);
                if(containsIndexes && member == "@")
                {
                    if(!type.IsArray)
                        throw new InvalidOperationException("An array expected");
                    il.Dup(); // stack: [arr, arr]
                    il.Ldlen(); // stack: [arr, arr.Length]
                    il.Ldarg(1); // stack: [arr, arr.Length, indexes]
                    il.Ldc_I4(i); // stack: [arr, arr.Length, indexes, i]
                    il.Ldelem(typeof(int)); // stack: [arr, arr.Length, indexes[i]]
                    il.Dup(); // stack: [arr, arr.Length, indexes[i], indexes[i]]
                    il.Stloc(index); // index = indexes[i]; stack: [arr, arr.Length, index]
                    il.Ble(returnDefaultLabel, false); // if(arr.Length <= index) goto returnDefaultValue; stack: [arr]
                    il.Ldloc(index); // stack: [arr, index]
                    var elementType = type.GetElementType();
                    il.Ldelem(elementType); // stack: [arr[index]]
                    type = elementType;
                    ++i;
                }
                else
                {
                    var property = type.GetProperty(member, BindingFlags.Public | BindingFlags.Instance);
                    if(property == null)
                        throw new PropertyNotFoundByValueExtractorException(type, path, member);
                    var getter = property.GetGetMethod();
                    il.Call(getter, type);
                    type = property.PropertyType;
                }
            }
            if(type.IsValueType)
                il.Box(type);
            il.Ret();
            il.MarkLabel(returnDefaultLabel);
            il.Pop();
            if(!type.IsValueType)
                il.Ldnull();
            else
            {
                var result = il.DeclareLocal(type);
                il.Ldloca(result);
                il.Initobj(type);
                il.Ldloc(result);
                il.Box(type);
            }
            il.Ret();
            var delegateType = containsIndexes ? typeof(Func<object, int[], object>) : typeof(Func<object, object>);
            return method.CreateDelegate(delegateType);
        }

        private static readonly Hashtable extractors = new Hashtable();
        private static readonly object extractorsLock = new object();

        private class PropertyNotFoundByValueExtractorException : Exception
        {
            public PropertyNotFoundByValueExtractorException(Type type, string path, string propertyName)
                : base(string.Format("Property '{0}' not found by {1} for (Type {2}, path {3})", propertyName, typeof(ObjectValueExtractor).Name, type.Name, path))
            {
            }
        }
    }
}