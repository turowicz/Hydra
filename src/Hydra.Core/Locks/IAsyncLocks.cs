using System;
using System.Threading;

namespace Hydra.Core.Locks
{
    public interface IAsyncLocks<T>
    {
        SemaphoreSlim Get(String collection, T key);
    }
}