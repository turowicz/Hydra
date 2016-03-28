using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hydra.Events
{
    public interface IStream
    {
        String Container { get; set; }

        Task WriteEventAsync(String shardingKey, String streamId, String eventData, StreamOptions options = default(StreamOptions), CancellationToken token = default(CancellationToken));

        Task<string[]> ReadEventsAsync(String shardingKey, String streamId, StreamOptions options = default(StreamOptions), CancellationToken token = default(CancellationToken));
    }
}