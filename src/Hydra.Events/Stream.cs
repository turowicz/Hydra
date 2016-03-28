using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public byte[] Delimiter { get; set; }

        private readonly IHydra _hydra;

        public Stream(IHydra hydra, String container = "hydra", String delimeter = "\r\n")
        {
            _hydra = hydra;
            Container = container;
            Delimiter = Encoding.UTF8.GetBytes(delimeter);
        }

        public async Task WriteAsync(String shardingKey, String streamId, System.IO.Stream dataStream, StreamOptions options = default(StreamOptions), CancellationToken token = default(CancellationToken))
        {
            var streamOptions = options ?? new StreamOptions();

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

            await blob.AppendBlockAsync(dataStream, null, token);

            if (streamOptions.AppendDelimeter)
            {
                await AppendDelimeter(blob, token);
            }
        }

        public async Task<IEnumerable<byte[]>> ReadAsync()
        {
            return Enumerable.Empty<byte[]>();
        }

        async Task AppendDelimeter(CloudAppendBlob blob, CancellationToken token)
        {
            using (var stream = new MemoryStream(Delimiter))
            {
                await blob.AppendBlockAsync(stream, null, token);
            }
        }
    }
}
