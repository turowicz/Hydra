using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hydra.Core;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Hydra.Events
{
    public class Stream
    {
        public String Container { get; set; }

        private static readonly byte[] Delimiter = Encoding.UTF8.GetBytes("\r\n");

        private static readonly string DelimiterString = "\r\n";

        private readonly IHydra _hydra;

        public Stream(IHydra hydra, String container = "hydra")
        {
            _hydra = hydra;
            Container = container;
        }

        public async Task WriteAsync(String shardingKey, String streamId, String eventData, StreamOptions options = default(StreamOptions), CancellationToken token = default(CancellationToken))
        {
            var streamOptions = options ?? new StreamOptions();
            var blob = await GetBlobReference(shardingKey, streamId, token, streamOptions);

            using (var dataStream = new MemoryStream(Encoding.UTF8.GetBytes(eventData)))
            {
                await blob.AppendBlockAsync(dataStream, null, token);
            }

            if (streamOptions.AppendDelimeter)
            {
                await AppendDelimeter(blob, token);
            }
        }

        public async Task<string[]> ReadAsync(String shardingKey, String streamId, StreamOptions options = default(StreamOptions), CancellationToken token = default(CancellationToken))
        {
            var streamOptions = options ?? new StreamOptions();
            var blob = await GetBlobReference(shardingKey, streamId, token, streamOptions);
            var content = await blob.DownloadTextAsync(Encoding.UTF8, null, null, null, token);

            return content.Split(new[] { DelimiterString }, StringSplitOptions.None);
        }

        async Task AppendDelimeter(CloudAppendBlob blob, CancellationToken token)
        {
            using (var stream = new MemoryStream(Delimiter))
            {
                await blob.AppendBlockAsync(stream, null, token);
            }
        }

        private async Task<CloudAppendBlob> GetBlobReference(string shardingKey, string streamId, CancellationToken token, StreamOptions streamOptions)
        {
            var client = _hydra.CreateBlobClient(shardingKey);
            var container = client.GetContainerReference(Container);

            if (streamOptions.CreateContainer)
            {
                await container.CreateIfNotExistsAsync(token);
            }

            var blob = container.GetAppendBlobReference(streamId);

            if (streamOptions.CreateBlob && !await blob.ExistsAsync(token))
            {
                await blob.CreateOrReplaceAsync(token);
            }

            return blob;
        }
    }
}
