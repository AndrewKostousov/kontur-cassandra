using System.Collections.Generic;

namespace SKBKontur.Catalogue.Objects.ValueExtracting
{
    public interface IObjectPropertyValueExtractor
    {
        KeyValuePair<string, object> Extract(object obj);
    }
}