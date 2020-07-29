using System;

namespace Dfe.Spi.EntitySquasher.Domain.Configuration
{
    public class EntitySquasherConfiguration
    {
        public SpiDataAdapterConfiguration Gias { get; set; }
        public SpiDataAdapterConfiguration Ukrlp { get; set; }
        public SpiDataAdapterConfiguration Rates { get; set; }
        public SpiDataAdapterConfiguration IStore { get; set; }
        public ProfileConfiguration Profile { get; set; }
    }

    public class SpiDataAdapterConfiguration
    {
        public string Url { get; set; }
        public string SubscriptionKey { get; set; }
    }

    public class ProfileConfiguration
    {
        public string BlobConnectionString { get; set; }
        public string ContainerName { get; set; }
        public int? CacheDurationSeconds { get; set; }
    }
}