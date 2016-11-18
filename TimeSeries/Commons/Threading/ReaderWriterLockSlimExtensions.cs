using System;
using System.Threading;

using SKBKontur.Catalogue.Threading.Wrappers;

namespace SKBKontur.Catalogue.Threading
{
    public static class ReaderWriterLockSlimExtensions
    {
        public static IDisposable TakeReadLock(this ReaderWriterLockSlim lockObject)
        {
            return new ReaderWriterLockSlimReadWrapper(lockObject);
        }

        public static IDisposable TakeWriteLock(this ReaderWriterLockSlim lockObject)
        {
            return new ReaderWriterLockSlimWriteWrapper(lockObject);
        }
    }
}