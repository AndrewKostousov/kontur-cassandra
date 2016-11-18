using System;
using System.Collections.Generic;

namespace SKBKontur.Catalogue.Objects.ValueExtracting
{
    public class CustomObjectPropertyValueExtractor : IObjectPropertyValueExtractor
    {
        public CustomObjectPropertyValueExtractor(Func<object, object> extractFunction, string propertyName)
        {
            this.extractFunction = extractFunction;
            this.propertyName = propertyName;
        }

        public KeyValuePair<string, object> Extract(object obj)
        {
            return new KeyValuePair<string, object>(propertyName, extractFunction(obj));
        }

        private readonly Func<object, object> extractFunction;
        private readonly string propertyName;
    }
}