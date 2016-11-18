using System;
using System.Collections.Generic;
using System.Linq;

namespace SKBKontur.Catalogue.Strings
{
    public static class StringExtensions
    {
        public static string NullIfWhitespace(this string s)
        {
            return string.IsNullOrWhiteSpace(s) ? null : s;
        }

        public static IEnumerable<string> SplitIntoPieces(this string source, int pieceLength)
        {
            if (pieceLength <= 0)
                throw new ArgumentException(string.Format("pieceLength should be positive integer. '{0}' is invalid value", pieceLength), "pieceLength");
            if(string.IsNullOrEmpty(source))
                yield break;
            for (var position = 0; position < source.Length; position += pieceLength)
            {
                var currentLength = Math.Min(pieceLength, source.Length - position);
                yield return source.Substring(position, currentLength);
            }
        }
    }
}