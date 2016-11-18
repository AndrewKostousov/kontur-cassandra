using System;
using System.Collections.Generic;

namespace SKBKontur.Catalogue.Ranges
{
    public static class Range
    {
        public static Range<T> Of<T>(T lowerBound, T upperBound)
        {
            return new Range<T>(lowerBound, upperBound);
        }

        public static Range<DateTime> OfDate(DateTime? lowerBound, DateTime? upperBound, DateTimeRangeOptions options = DateTimeRangeOptions.None)
        {
            if (lowerBound.HasValue && upperBound.HasValue)
                return Range.OfOrEmpty(lowerBound.Value, upperBound.Value, options);
            if (!lowerBound.HasValue && upperBound.HasValue)
                return new Range<DateTime>(options == DateTimeRangeOptions.FullDays ? EndOfDay(upperBound.Value) : upperBound.Value, RangeOpenType.LowerBoundOpen);
            if (lowerBound.HasValue)
                return new Range<DateTime>(options == DateTimeRangeOptions.FullDays ? StartOfDay(lowerBound.Value) : lowerBound.Value, RangeOpenType.UpperBoundOpen);
            return new Range<DateTime>(RangeOpenType.BothBoundOpened);
        }

        public static Range<T> OfOrEmpty<T>(T lowerBound, T upperBound)
        {
            try
            {
                return new Range<T>(lowerBound, upperBound);
            }
            catch
            {
                return Range<T>.Empty;
            }
        }

        public static Range<DateTime> OfOrEmpty(DateTime lowerBound, DateTime upperBound, DateTimeRangeOptions options)
        {
            try
            {
                if(options == DateTimeRangeOptions.FullDays)
                    return new Range<DateTime>(StartOfDay(lowerBound), EndOfDay(upperBound));
                return new Range<DateTime>(lowerBound, upperBound);
            }
            catch
            {
                return Range<DateTime>.Empty;
            }
        }

        public static Range<DateTime> OfOrEmpty(DateTime lowerBound, DateTime upperBound, TimeSpan extendTimeSpan, DateTimeRangeOptions options)
        {
            try
            {
                if(options == DateTimeRangeOptions.FullDays)
                    return new Range<DateTime>(StartOfDay(lowerBound - extendTimeSpan), EndOfDay(upperBound + extendTimeSpan));
                return new Range<DateTime>(lowerBound - extendTimeSpan, upperBound + extendTimeSpan);
            }
            catch
            {
                return Range<DateTime>.Empty;
            }
        }

        private static DateTime EndOfDay(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999);
        }

        private static DateTime StartOfDay(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0);
        }
    }

    public enum RangeOpenType
    {
        UpperBoundOpen,
        LowerBoundOpen,
        BothBoundOpened
    }

    public sealed class Range<T>
    {
        public Range(T lowerBound, T upperBound, IComparer<T> comparer = null)
        {
            empty = false;
            OpenType = null;
            this.lowerBound = lowerBound;
            this.upperBound = upperBound;

            Comparer = comparer ?? Comparer<T>.Default;

            if(Comparer.Compare(LowerBound, UpperBound) > 0)
                throw new ArgumentException("Invalid bounds");
        }

        public Range(T point, RangeOpenType openType, IComparer<T> comparer = null)
        {
            empty = false;
            OpenType = openType;
            if(openType == RangeOpenType.LowerBoundOpen)
            {
                lowerBound = default(T);
                upperBound = point;
            }
            else if(openType == RangeOpenType.UpperBoundOpen)
            {
                lowerBound = point;
                upperBound = default(T);
            }
            else
                throw new ArgumentException("This constructor overload accept only single side open type", "openType");

            Comparer = comparer ?? Comparer<T>.Default;
        }

        public Range(RangeOpenType openType, IComparer<T> comparer = null)
        {
            empty = false;
            OpenType = openType;
            if(openType != RangeOpenType.BothBoundOpened)
                throw new ArgumentException("This constructor overload accept only both side open type", "openType");
            lowerBound = default(T);
            upperBound = default(T);

            Comparer = comparer ?? Comparer<T>.Default;
        }

        private Range()
        {
            empty = true;
        }

        public bool IsEmpty { get { return empty; } }

        public bool Contains(T item)
        {
            if(empty)
                return false;
            if(OpenType.HasValue && OpenType.Value == RangeOpenType.BothBoundOpened)
                return true;
            if(!(OpenType.HasValue && OpenType.Value == RangeOpenType.LowerBoundOpen))
            {
                var lowerCompare = Comparer.Compare(LowerBound, item);
                if(lowerCompare > 0)
                    return false;
            }
            if(!(OpenType.HasValue && OpenType.Value == RangeOpenType.UpperBoundOpen))
            {
                var upperCompare = Comparer.Compare(item, UpperBound);
                if(upperCompare > 0)
                    return false;
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj)) return false;
            if(ReferenceEquals(this, obj)) return true;
            return obj is Range<T> && Equals((Range<T>)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = empty.GetHashCode();
                hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(LowerBound);
                hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(UpperBound);
                return hashCode;
            }
        }

        public static Range<T> Empty
        {
            get
            {
                if(emptyInstance == null)
                {
                    lock(emptyInstanceLock)
                    {
                        if(emptyInstance == null)
                            emptyInstance = new Range<T>();
                    }
                }
                return emptyInstance;
            }
        }

        public RangeOpenType? OpenType { get; private set; }

        public T LowerBound
        {
            get
            {
                if(OpenType.HasValue && (OpenType.Value == RangeOpenType.LowerBoundOpen || OpenType.Value == RangeOpenType.BothBoundOpened))
                    throw new RangeDoesNotHaveSpecifiedBoundException();
                return lowerBound;
            }
        }

        public T UpperBound
        {
            get
            {
                if(OpenType.HasValue && (OpenType.Value == RangeOpenType.UpperBoundOpen || OpenType.Value == RangeOpenType.BothBoundOpened))
                    throw new RangeDoesNotHaveSpecifiedBoundException();
                return upperBound;
            }
        }

        public IComparer<T> Comparer { get; private set; }

        private bool Equals(Range<T> other)
        {
            if(!empty.Equals(other.empty) || !OpenType.Equals(other.OpenType)) 
                return false;
            switch(OpenType)
            {
            case RangeOpenType.UpperBoundOpen:
                return EqualityComparer<T>.Default.Equals(LowerBound, other.LowerBound);
            case RangeOpenType.LowerBoundOpen:
                return EqualityComparer<T>.Default.Equals(UpperBound, other.UpperBound);
            case RangeOpenType.BothBoundOpened:
                return true;
            case null:
                return EqualityComparer<T>.Default.Equals(LowerBound, other.LowerBound) && EqualityComparer<T>.Default.Equals(UpperBound, other.UpperBound);
            default:
                throw new ArgumentOutOfRangeException();
            }
        }

        private readonly bool empty;
        private readonly T lowerBound;
        private readonly T upperBound;

        private static volatile Range<T> emptyInstance;
        // ReSharper disable StaticFieldInGenericType
        private static volatile object emptyInstanceLock = new object();
        // ReSharper restore StaticFieldInGenericType
    }
}