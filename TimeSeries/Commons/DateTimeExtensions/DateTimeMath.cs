using System;

namespace SKBKontur.Catalogue.DateTimeExtensions
{
    public static class DateTimeMath
    {
        public static TimeSpan Max(TimeSpan first, TimeSpan second)
        {
            return first >= second ? first : second;
        }
    }
}