using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Hydra.Events
{
    public interface IStreamContainer
    {
        Task<CloudAppendBlob> GetBlobReference(String shardingKey, String containerName, String streamId, CancellationToken token, StreamOptions streamOptions);
    }
}