using System;

namespace SKBKontur.Catalogue.Ranges
{
    public static class DateTimeRangeExtensions
    {
        public static Range<DateTime> WholeCurrentYear(this DateTime dateTime)
        {
            return Range.Of(
                new DateTime(dateTime.Year, 1, 1, 0, 0, 0),
                new DateTime(dateTime.Year + 1, 1, 1, 0, 0, 0) - TimeSpan.FromTicks(1));
        }

        public static DateTime? GetUpperBoundOrNull(this Range<DateTime> dateTimeRange)
        {
            switch(dateTimeRange.OpenType)
            {
            case RangeOpenType.UpperBoundOpen:
            case RangeOpenType.BothBoundOpened:
                return null;
            default:
                return dateTimeRange.UpperBound;
            }
        }
        
        public static DateTime? GetLowerBoundOrNull(this Range<DateTime> dateTimeRange)
        {
            switch(dateTimeRange.OpenType)
            {
            case RangeOpenType.LowerBoundOpen:
            case RangeOpenType.BothBoundOpened:
                return null;
            default:
                return dateTimeRange.LowerBound;
            }
        }

        public static Range<DateTime> Expand(this DateTime dateTimePoint, TimeSpan interval)
        {
            return Range.Of(dateTimePoint - interval, dateTimePoint + interval);
        }

        public static Range<DateTime> Offset(this Range<DateTime> dateTimePoint, TimeSpan offsetValue)
        {
            return Range.Of(dateTimePoint.LowerBound + offsetValue, dateTimePoint.UpperBound + offsetValue);
        }
    }
}