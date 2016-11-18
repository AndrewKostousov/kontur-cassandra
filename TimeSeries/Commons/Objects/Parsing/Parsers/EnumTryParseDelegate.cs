using System;

namespace SKBKontur.Catalogue.Objects.Parsing.Parsers
{
    public delegate bool EnumTryParseDelegate(Type enumType, string value, out object result);
}