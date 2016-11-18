using System;

namespace SKBKontur.Catalogue.Objects.Parsing
{
    public class ObjectParser : IObjectParser
    {
        public ObjectParser()
            : this(new ObjectParserCollectionNullableWrapper(ObjectParserConfiguration.CreateDefault()))
        {
        }
        
        //todo сделать публичным, но не использовать ContainerConstructor
        private ObjectParser(IObjectParserCollection collection)
        {
            this.collection = collection;
        }

        public object Parse(Type type, string value)
        {
            object result;
            if(type.IsEnum)
            {
                if(collection.GetEnumParser().TryParse(type, value, out result))
                    return result;
                return Default(type);
            }

            if(collection.GetParser(type).TryParse(type, value, out result))
                return result;
            return Default(type);
        }

        private object Default(Type type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);
            return null;
        }

        private readonly IObjectParserCollection collection;
    }
}