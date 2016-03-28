using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Hydra.Tests.Integration
{
    public class EventTests : IntegrationBase
    {
        [Fact]
        public async Task WritesNewStream()
        {
            var hydra = CreateHydraMock();
            var subject = new Events.Stream(hydra.Object);

            var eventData = "{ \"name\": \"john\" }";

            using (var dataStream = new MemoryStream(Encoding.UTF8.GetBytes(eventData)))
            {
                await subject.WriteAsync("key", Guid.NewGuid().ToString(), dataStream);
            }
        }

        [Fact]
        public async Task WritesExistingStream()
        {
            var hydra = CreateHydraMock();
            var subject = new Events.Stream(hydra.Object);

            var id = Guid.NewGuid().ToString();
            var eventData1 = "{ \"name\": \"john\" }";
            var eventData2 = "{ \"lastname\": \"doe\" }";

            using (var dataStream = new MemoryStream(Encoding.UTF8.GetBytes(eventData1)))
            {
                await subject.WriteAsync("key", id, dataStream);
            }

            using (var dataStream = new MemoryStream(Encoding.UTF8.GetBytes(eventData2)))
            {
                await subject.WriteAsync("key", id, dataStream);
            }
        }
    }
}
