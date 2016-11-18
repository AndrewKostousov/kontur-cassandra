using System;
using System.Linq;

namespace SKBKontur.Catalogue.Objects.Parsing.Exceptions
{
    public class ParserNotFoundException : Exception
    {
        public ParserNotFoundException(Type t)
            : base(string.Format("Parser for type '{0}' not found.", GetFriendlyName(t)))
        {
        }

        public ParserNotFoundException(string parserDescription)
            : base(string.Format("Parser '{0}' not found.", parserDescription))
        {
        }

        private static string GetFriendlyName(Type type)
        {
            if(type == typeof(int))
                return "int";
            if(type == typeof(short))
                return "short";
            if(type == typeof(byte))
                return "byte";
            if(type == typeof(bool))
                return "bool";
            if(type == typeof(long))
                return "long";
            if(type == typeof(float))
                return "float";
            if(type == typeof(double))
                return "double";
            if(type == typeof(decimal))
                return "decimal";
            if(type == typeof(string))
                return "string";
            if(type.IsGenericType)
                return type.Name.Split('`')[0] + "<" + string.Join(", ", type.GetGenericArguments().Select(GetFriendlyName)) + ">";
            return type.Name;
        }
    }
}