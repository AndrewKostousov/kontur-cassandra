using System;

namespace SKBKontur.Catalogue.Objects.Parsing
{
    public interface IObjectParser
    {
        object Parse(Type type, string value);
    }
}