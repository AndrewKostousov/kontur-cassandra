using System.Collections.Generic;
using System.Linq;

namespace CassandraTimeSeries.ReadWrite
{
    public class ReadersWritersPool<TReader, TWriter>
        where TReader : EventReader 
        where TWriter : EventWriter
    {
        private readonly WorkersPool readersPool;
        private readonly WorkersPool writersPool;
        private bool isAlive;

        public IReadOnlyList<TReader> Readers { get; }
        public IReadOnlyList<TWriter> Writers { get; }

        public ReadersWritersPool(IReadOnlyList<TReader> readers, IReadOnlyList<TWriter> writers)
        {
            Readers = readers;
            Writers = writers;

            readersPool = new WorkersPool(readers.Count, i =>
            {
                readers[i].ReadFirst();

                while (isAlive)
                    readers[i].ReadNext();
            });

            writersPool = new WorkersPool(writers.Count, i =>
            {
                while (isAlive)
                    writers[i].WriteNext();
            });
        }

        public void Start()
        {
            isAlive = true;
            readersPool.Start();
            writersPool.Start();
        }

        public void Stop()
        {
            isAlive = false;
            readersPool.Join();
            writersPool.Join();
        }
    }
}
