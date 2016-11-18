using System;

namespace SKBKontur.Catalogue.Objects
{
    [Flags]
    public enum ResultOptions
    {
        None = 0x00,
        NullifyEmptyStrings = 0x01,
        TrimSpaces = 0x02,
        NullifyWhitespaceStrings = 0x04
    }
}