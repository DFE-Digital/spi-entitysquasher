using System;
using Dfe.Spi.EntitySquasher.Domain.Definitions.SettingsProviders;

namespace Dfe.Spi.EntitySquasher.FunctionApp.SettingsProviders
{
    public class EntityAdapterSettingsProvider : IEntityAdapterSettingsProvider
    {
        /// <inheritdoc />
        public string GiasBaseUrl => Environment.GetEnvironmentVariable(nameof(this.GiasBaseUrl));

        /// <inheritdoc />
        public string UkrlpBaseUrl => Environment.GetEnvironmentVariable(nameof(this.UkrlpBaseUrl));

        /// <inheritdoc />
        public string RatesBaseUrl => Environment.GetEnvironmentVariable(nameof(this.RatesBaseUrl));

        /// <inheritdoc />
        public string IStoreBaseUrl => Environment.GetEnvironmentVariable(nameof(this.IStoreBaseUrl));

        /// <inheritdoc />
        public string SubscriptionKey => Environment.GetEnvironmentVariable(nameof(this.SubscriptionKey));
    }
}