using System;
using System.Threading;

namespace SKBKontur.Catalogue.Threading.Wrappers
{
    internal class ReaderWriterLockSlimReadWrapper : IDisposable
    {
        public ReaderWriterLockSlimReadWrapper(ReaderWriterLockSlim lockObject)
        {
            this.lockObject = lockObject;
            lockObject.EnterReadLock();
        }

        public void Dispose()
        {
            lockObject.ExitReadLock();
        }

        private readonly ReaderWriterLockSlim lockObject;
    }
}