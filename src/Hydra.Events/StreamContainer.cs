using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hydra.Core;
using Hydra.Core.Locks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Hydra.Events
{
    public class StreamContainer : IStreamContainer
    {
        private static readonly IDictionary<String, object> ExistingContainers = new ConcurrentDictionary<String, object>();

        private static readonly IDictionary<String, object> ExistingStreams = new ConcurrentDictionary<String, object>();

        private static readonly IAsyncLocks<String> Locks = new AsyncLocks<String>();

        private readonly IHydra _hydra;

        public StreamContainer(IHydra hydra)
        {
            _hydra = hydra;
        }

        public async Task<CloudAppendBlob> GetBlobReference(String shardingKey, String containerName, String streamId, CancellationToken token, StreamOptions streamOptions)
        {
            var client = _hydra.CreateBlobClient(shardingKey);
            var account = client.Credentials.AccountName;
            var container = client.GetContainerReference(containerName);
            var semaphore = GetSemaphore(account, containerName, streamId);

            if (await semaphore.WaitAsync(TimeSpan.FromSeconds(5), token))
            {
                try
                {
                    if (streamOptions.CreateContainer && !GetContainerExists(account, containerName))
                    {
                        await container.CreateIfNotExistsAsync(token);
                        SetContainerExists(account, containerName);
                    }

                    var blob = container.GetAppendBlobReference(streamId);

                    if (streamOptions.CreateBlob && !GetStreamExists(account, containerName, streamId) && !await blob.ExistsAsync(token))
                    {
                        await blob.CreateOrReplaceAsync(token);
                        SetStreamExists(account, containerName, streamId);
                    }

                    return blob;
                }
                finally
                {
                    semaphore.Release();
                }
            }

            throw new TimeoutException("Unable to get blob reference");
        }

        private static SemaphoreSlim GetSemaphore(String account, String container, String blob)
        {
            return Locks.Get($"{account}-{container}", blob);
        }

        private static Boolean GetContainerExists(String account, String container)
        {
            return ExistingContainers.ContainsKey($"{account}-{container}");
        }

        private static void SetContainerExists(String account, String container)
        {
            ExistingContainers[$"{account}-{container}"] = true;
        }

        private static Boolean GetStreamExists(String account, String container, String blob)
        {
            return ExistingStreams.ContainsKey($"{account}-{container}-{blob}");
        }

        private static void SetStreamExists(String account, String container, String blob)
        {
            ExistingStreams[$"{account}-{container}-{blob}"] = true;
        }
    }
}
