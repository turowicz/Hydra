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

        private static readonly IDictionary<string, object> ExistingContainers = new ConcurrentDictionary<string, object>();

        private static readonly IDictionary<string, object> ExistingStreams = new ConcurrentDictionary<string, object>();

        private readonly IHydra _hydra;

        public StreamContainer(IHydra hydra)
        {
            _hydra = hydra;
        }

        public async Task<CloudAppendBlob> GetBlobReference(string shardingKey, string containerName, string streamId, CancellationToken token, StreamOptions streamOptions)
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

        private static Boolean GetContainerExists(string account, string container)
        {
            return ExistingContainers.ContainsKey($"{account}-{container}");
        }

        private static void SetContainerExists(string account, string container)
        {
            ExistingContainers[$"{account}-{container}"] = true;
        }

        private static Boolean GetStreamExists(string account, string container, string blob)
        {
            return ExistingStreams.ContainsKey($"{account}-{container}-{blob}");
        }

        private static void SetStreamExists(string account, string container, string blob)
        {
            ExistingStreams[$"{account}-{container}-{blob}"] = true;
        }
    }
}
