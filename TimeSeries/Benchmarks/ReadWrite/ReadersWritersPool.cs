using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Benchmarks.ReadWrite
{
    class ReadersWritersPool
    {
        private readonly WorkersPool readersPool;
        private readonly WorkersPool writersPool;
        private bool isAlive;

        public ReadersWritersPool(IEnumerable<EventReader> readers, IEnumerable<EventWriter> writers)
        {
            var readersList = readers.ToList();
            var writersList = writers.ToList();

            readersPool = new WorkersPool(readersList.Count, i =>
            {
                readersList[i].ReadFirst();

                while (isAlive)
                    readersList[i].ReadNext();
            });

            writersPool = new WorkersPool(writersList.Count, i =>
            {
                while (isAlive)
                    writersList[i].WriteNext();
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
