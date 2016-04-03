using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hydra.Core;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Hydra.Events
{
    public class StreamContainer : IStreamContainer
    {
        private static readonly IDictionary<String, object> ExistingContainers = new ConcurrentDictionary<String, object>();

        private static readonly IDictionary<String, object> ExistingStreams = new ConcurrentDictionary<String, object>();

        private readonly IHydra _hydra;

        public StreamContainer(IHydra hydra)
        {
            _hydra = hydra;
        }

        public async Task<CloudAppendBlob> GetBlobReference(String shardingKey, String containerName, String streamId, CancellationToken token, StreamOptions streamOptions)
        {
            var client = _hydra.CreateBlobClient(shardingKey);
            var container = client.GetContainerReference(containerName);

            if (streamOptions.CreateContainer && !GetContainerExists(client.Credentials.AccountName, containerName))
            {
                await container.CreateIfNotExistsAsync(token);
                SetContainerExists(client.Credentials.AccountName, containerName);
            }

            var blob = container.GetAppendBlobReference(streamId);

            if (streamOptions.CreateBlob && !GetStreamExists(client.Credentials.AccountName, containerName, streamId) && !await blob.ExistsAsync(token))
            {
                await blob.CreateOrReplaceAsync(token);
                SetStreamExists(client.Credentials.AccountName, containerName, streamId);
            }

            return blob;
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
