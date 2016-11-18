namespace SKBKontur.Catalogue.Objects.Parsing.Parsers
{
    public delegate bool TryParseDelegate<T>(string s, out T result);
}