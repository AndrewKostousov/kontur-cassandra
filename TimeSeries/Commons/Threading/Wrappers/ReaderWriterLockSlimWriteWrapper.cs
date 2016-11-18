using System;
using System.Threading;

namespace SKBKontur.Catalogue.Threading.Wrappers
{
    internal class ReaderWriterLockSlimWriteWrapper : IDisposable
    {
        public ReaderWriterLockSlimWriteWrapper(ReaderWriterLockSlim lockObject)
        {
            this.lockObject = lockObject;
            lockObject.EnterWriteLock();
        }

        public void Dispose()
        {
            lockObject.ExitWriteLock();
        }

        private readonly ReaderWriterLockSlim lockObject;
    }
}