namespace EdiTimeline
{
    public interface IAllBoxEventSeriesTicksHolder
    {
        long? GetEventSeriesExclusiveStartTicks();
        void SetEventSeriesExclusiveStartTicks(long nowTicks);
        long? GetLastGoodEventTicks();
        void SetLastGoodEventTicks(long eventTicks);
    }
}