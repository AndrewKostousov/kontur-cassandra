using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.Model
{
    class StatementExecutionResult
    {
        public ExecutionState State { get; set; }
        public TimeGuid PartitionMaxGuid { get; set; }
    }

    enum ExecutionState
    {
        Success,
        OutdatedId,
        PartitionClosed,
    }
}