using System;
using System.Linq;
using System.Threading.Tasks;
using Hydra.Events;
using Xunit;

namespace Hydra.Tests.Integration
{
    public class EventTests : IntegrationBase
    {
        [Fact]
        public async Task WritesReadsNewStream()
        {
            var hydra = CreateHydra();
            var container = new StreamContainer(hydra);
            var subject = new Stream(container);

            var id = Guid.NewGuid().ToString();
            var eventData = "{ \"name\": \"john\" }";

            await subject.WriteEventAsync("key", id, eventData);

            var persisted = await subject.ReadEventsAsync("key", id);

            Assert.Equal(eventData, persisted.First());
        }

        [Fact]
        public async Task WritesReadsExistingStream()
        {
            var hydra = CreateHydra();
            var container = new StreamContainer(hydra);
            var subject = new Stream(container);

            var id = Guid.NewGuid().ToString();
            var eventData1 = "{ \"name\": \"john\" }";
            var eventData2 = "{ \"lastname\": \"doe\" }";

            await subject.WriteEventAsync("key", id, eventData1);
            await subject.WriteEventAsync("key", id, eventData2);

            var persisted = await subject.ReadEventsAsync("key", id);

            Assert.Equal(eventData1, persisted[0]);
            Assert.Equal(eventData2, persisted[1]);
        }
    }
}
