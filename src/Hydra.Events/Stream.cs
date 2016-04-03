using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hydra.Core;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Hydra.Events
{
    public class Stream : IStream
    {
        public String Container { get; set; }

        private static readonly byte[] Delimiter = Encoding.UTF8.GetBytes("\r\n");

        private static readonly string DelimiterString = "\r\n";

        private static readonly IDictionary<string, object> ExistingContainers = new ConcurrentDictionary<string, object>();

        private static readonly IDictionary<string, object> ExistingStreams = new ConcurrentDictionary<string, object>();

        private readonly IHydra _hydra;

        public Stream(IHydra hydra, String container = "hydra")
        {
            _hydra = hydra;
            Container = container;
        }

        public async Task WriteEventAsync(String shardingKey, String streamId, String eventData, StreamOptions options = default(StreamOptions), CancellationToken token = default(CancellationToken))
        {
            var dataBuilder = new StringBuilder(eventData);
            var streamOptions = options ?? new StreamOptions();
            var blob = await GetBlobReference(shardingKey, streamId, token, streamOptions);

            if (streamOptions.AppendDelimeter)
            {
                dataBuilder.Append(DelimiterString);
            }

            using (var dataStream = new MemoryStream(Encoding.UTF8.GetBytes(dataBuilder.ToString())))
            {
                await blob.AppendBlockAsync(dataStream, null, token);
            }
        }

        public async Task<string[]> ReadEventsAsync(String shardingKey, String streamId, StreamOptions options = default(StreamOptions), CancellationToken token = default(CancellationToken))
        {
            var streamOptions = options ?? new StreamOptions();
            var blob = await GetBlobReference(shardingKey, streamId, token, streamOptions);
            var content = await blob.DownloadTextAsync(Encoding.UTF8, null, null, null, token);

            return content.Split(new[] { DelimiterString }, StringSplitOptions.None);
        }

        private async Task<CloudAppendBlob> GetBlobReference(string shardingKey, string streamId, CancellationToken token, StreamOptions streamOptions)
        {
            var client = _hydra.CreateBlobClient(shardingKey);
            var container = client.GetContainerReference(Container);

            if (streamOptions.CreateContainer && !GetContainerExists(client.Credentials.AccountName, Container))
            {
                await container.CreateIfNotExistsAsync(token);
                SetContainerExists(client.Credentials.AccountName, Container);
            }

            var blob = container.GetAppendBlobReference(streamId);

            if (streamOptions.CreateBlob && !GetStreamExists(client.Credentials.AccountName, Container, streamId) && !await blob.ExistsAsync(token))
            {
                await blob.CreateOrReplaceAsync(token);
                SetStreamExists(client.Credentials.AccountName, Container, streamId);
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
