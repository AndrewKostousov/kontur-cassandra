using System;

namespace SKBKontur.Catalogue.Objects.Parsing.Parsers.InternalImplementations
{
    internal class EnumParser : IValueParser
    {
        public EnumParser(EnumTryParseDelegate tryParse)
        {
            this.tryParse = tryParse;
        }

        public bool TryParse(Type enumType, string value, out object result)
        {
            return tryParse(enumType, value, out result);
        }

        private readonly EnumTryParseDelegate tryParse;
    }
}