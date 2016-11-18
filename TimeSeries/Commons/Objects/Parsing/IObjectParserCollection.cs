using System;

using SKBKontur.Catalogue.Objects.Parsing.Parsers;

namespace SKBKontur.Catalogue.Objects.Parsing
{
    public interface IObjectParserCollection
    {
        IValueParser GetParser(Type type);
        IValueParser GetEnumParser();
    }
}