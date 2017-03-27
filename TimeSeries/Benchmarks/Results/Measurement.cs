using System;
using System.Runtime.Serialization;
using Commons;

namespace Benchmarks.Results
{

    [DataContract]
    public class Measurement
    {
        [DataMember]
        public long LatencyMilliseconds { get; set; }

        [DataMember]
        public long StartMilliseconds { get; set; }

        [DataMember]
        public int Throughput { get; set; }

        public long StopMilliseconds { get; }

        private Measurement(long startMilliseconds, long stopMilliseconds, int throughput)
        {
            if (startMilliseconds > stopMilliseconds)
                throw new ArgumentException($"Expected {nameof(startMilliseconds)} to be less or equal to {nameof(stopMilliseconds)}");

            StopMilliseconds = stopMilliseconds;
            StartMilliseconds = startMilliseconds;
            LatencyMilliseconds = StopMilliseconds - StartMilliseconds;
            Throughput = throughput;
        }

        public static RunningMeasurement Start()
        {
            return new RunningMeasurement(GetTimeTotalMilliseconds());
        }

        public class RunningMeasurement
        {
            private readonly long startMilliseconds;

            public RunningMeasurement(long startMilliseconds)
            {
                this.startMilliseconds = startMilliseconds;
            }

            public Measurement Stop(int throughput)
            {
                return new Measurement(startMilliseconds, GetTimeTotalMilliseconds(), throughput);
            }
        }

        private static long GetTimeTotalMilliseconds()
        {
            return Timestamp.Now.Ticks/TimeSpan.TicksPerMillisecond;
        }
    }
}
