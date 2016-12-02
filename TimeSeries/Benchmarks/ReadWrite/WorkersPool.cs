using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Benchmarks.ReadWrite
{
    class WorkersPool
    {
        private readonly List<Thread> workers;

        public WorkersPool(int workersCount, Action<int> workerAction)
        {
            workers = Enumerable.Range(0, workersCount)
                .Select(i => new Thread(() => workerAction(i)))
                .ToList();
        }

        public void Start()
        {
            foreach (var worker in workers)
                worker.Start();
        }

        public void Join()
        {
            foreach (var worker in workers)
                worker.Join();
        }
    }
}