using System;

namespace SKBKontur.Catalogue.Objects.Parsing.Parsers
{
    public interface IValueParser
    {
        bool TryParse(Type type, string value, out object result);
    }
}