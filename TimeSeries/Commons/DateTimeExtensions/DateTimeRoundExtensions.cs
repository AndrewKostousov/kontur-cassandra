using System;
using System.Collections.Generic;

namespace SKBKontur.Catalogue.DateTimeExtensions
{
    public static class DateTimeRoundExtensions
    {
        public static DateTime Floor(this DateTime dateTime, TimeSpan roundSpan)
        {
            var ticks = dateTime.Ticks / roundSpan.Ticks;
            return new DateTime(ticks * roundSpan.Ticks);
        }

        public static DateTime Floor(this DateTime dateTime, DateTimeField dateTimeField)
        {
            return dateTime.Floor(GetRoundSpanByField(dateTimeField));
        }

        public static DateTime Round(this DateTime dateTime, TimeSpan roundSpan)
        {
            var ticks = (dateTime.Ticks + (roundSpan.Ticks / 2) + 1) / roundSpan.Ticks;
            return new DateTime(ticks * roundSpan.Ticks);
        }

        public static DateTime Round(this DateTime dateTime, DateTimeField dateTimeField)
        {
            return dateTime.Round(GetRoundSpanByField(dateTimeField));
        }

        private static TimeSpan GetRoundSpanByField(DateTimeField dateTimeField)
        {
            return dateTimeFieldSpans[dateTimeField];
        }

        private static readonly Dictionary<DateTimeField, TimeSpan> dateTimeFieldSpans = new Dictionary<DateTimeField, TimeSpan>
            {
                {DateTimeField.Second, TimeSpan.FromSeconds(1)},
                {DateTimeField.Minute, TimeSpan.FromMinutes(1)},
                {DateTimeField.Hour, TimeSpan.FromHours(1)},
                {DateTimeField.Day, TimeSpan.FromDays(1)}
            };
    }
}