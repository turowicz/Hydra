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

        private static readonly String DelimiterString = "\r\n";

        private readonly IStreamContainer _streamContainer;

        public Stream(IStreamContainer streamContainer, String container = "hydra")
        {
            _streamContainer = streamContainer;
            Container = container;
        }

        public async Task WriteEventAsync(String shardingKey, String streamId, String eventData, StreamOptions options = default(StreamOptions), CancellationToken token = default(CancellationToken))
        {
            var dataBuilder = new StringBuilder(eventData);
            var streamOptions = options ?? new StreamOptions();
            var blob = await _streamContainer.GetBlobReference(shardingKey, Container, streamId, token, streamOptions);

            if (streamOptions.AppendDelimeter)
            {
                dataBuilder.Append(DelimiterString);
            }

            using (var dataStream = new MemoryStream(Encoding.UTF8.GetBytes(dataBuilder.ToString())))
            {
                await blob.AppendBlockAsync(dataStream, null, token);
            }
        }

        public async Task<String[]> ReadEventsAsync(String shardingKey, String streamId, StreamOptions options = default(StreamOptions), CancellationToken token = default(CancellationToken))
        {
            var streamOptions = options ?? new StreamOptions();
            var blob = await _streamContainer.GetBlobReference(shardingKey, Container, streamId, token, streamOptions);
            var content = await blob.DownloadTextAsync(Encoding.UTF8, null, null, null, token);

            return content.Split(new[] { DelimiterString }, StringSplitOptions.None);
        }
    }
}
