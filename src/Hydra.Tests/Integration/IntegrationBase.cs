using System;
using Hydra.Core;
using Hydra.Core.Sharding;
using Microsoft.WindowsAzure.Storage;

namespace Hydra.Tests.Integration
{
    public abstract class IntegrationBase
    {
        public const String TableName = "testtable";
        public const String ContainerName = "testcontainer";
        public const String QueueName = "testqueue";

        public static String TestKey = Guid.NewGuid().ToString();

        public static IHydra Subject { get; private set; }

        static IntegrationBase()
        {
            Subject = CreateHydra();
        }

        protected static IHydra CreateHydra()
        {
            var sharding = new JumpSharding();

            return Core.Hydra.Create(sharding, new[]
            {
                CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("HYDRATEST")),
                CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("HYDRATEST")),
                CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("HYDRATEST")),
                CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("HYDRATEST")),
                CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("HYDRATEST"))
            });
        }
    }
}
