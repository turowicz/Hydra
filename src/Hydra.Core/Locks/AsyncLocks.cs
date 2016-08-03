using System;
using System.Collections.Generic;
using System.Threading;

namespace Hydra.Core.Locks
{
    public class AsyncLocks<T> : IAsyncLocks<T>
    {
        Dictionary<String, Dictionary<T, SemaphoreSlim>> _locks = new Dictionary<String, Dictionary<T, SemaphoreSlim>>();

        Object _lock = new Object();

        public SemaphoreSlim Get(String collection, T key)
        {
            if (_locks.ContainsKey(collection) && _locks[collection].ContainsKey(key))
                return _locks[collection][key];

            lock (_lock)
            {
                if (!_locks.ContainsKey(collection))
                {
                    _locks[collection] = new Dictionary<T, SemaphoreSlim>();
                }

                if (!_locks[collection].ContainsKey(key))
                {
                    _locks[collection][key] = new SemaphoreSlim(1);
                }

                return _locks[collection][key];
            }
        }
    }
}
