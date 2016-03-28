using System;
using Hydra.Core.Sharding;
using Microsoft.WindowsAzure.Storage;
using Moq;
using Xunit;

namespace Hydra.Tests.Unit
{
    public class CoreTests : UnitBase
    {
        [Fact]
        public void AcceptsStorageAccounts()
        {
            var sut = new Core.Hydra();
            sut.AddAccount(CloudStorageAccount.DevelopmentStorageAccount);
            Assert.Contains(CloudStorageAccount.DevelopmentStorageAccount, sut.Accounts);
        }

        [Fact]
        public void AcceptsSharding()
        {
            var sharding = new Mock<ISharding>().Object;
            var sut = new Core.Hydra();
            sut.SetSharding(sharding);
            Assert.Equal(sharding, sut.Sharding);
        }

        [Fact]
        public void CreatesTableClient()
        {
            var client = Subject.CreateTableClient(It.IsAny<String>());

            Assert.NotNull(client);
            Assert.Equal(client.StorageUri, CloudStorageAccount.DevelopmentStorageAccount.TableStorageUri);
        }

        [Fact]
        public void CreatesBlobClient()
        {
            var client = Subject.CreateBlobClient(It.IsAny<String>());

            Assert.NotNull(client);
            Assert.Equal(client.StorageUri, CloudStorageAccount.DevelopmentStorageAccount.BlobStorageUri);
        }

        [Fact]
        public void CreatesQueueClient()
        {
            var client = Subject.CreateQueueClient(It.IsAny<String>());

            Assert.NotNull(client);
            Assert.Equal(client.StorageUri, CloudStorageAccount.DevelopmentStorageAccount.QueueStorageUri);
        }

        [Fact]
        public void CreatesAnalyticsClient()
        {
            var client = Subject.CreateAnalyticsClient(It.IsAny<String>());

            Assert.NotNull(client);
        }

        //TODO: Implement a test for file client
        //[Fact]
        //public void CreatesFileClient()
        //{
        //    var client = Subject.CreateFileClient(It.IsAny<String>());

        //    Assert.NotNull(client);
        //    Assert.Equal(client.StorageUri, CloudStorageAccount.DevelopmentStorageAccount.FileStorageUri);
        //}
    }
}
