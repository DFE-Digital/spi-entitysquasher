using System.Threading;
using Dfe.Spi.Common.WellKnownIdentifiers;
using Dfe.Spi.EntitySquasher.Domain.Definitions.SettingsProviders;

namespace Dfe.Spi.EntitySquasher.Infrastructure.EntityAdapter.Factories
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Dfe.Spi.Common.Context.Definitions;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Definitions.Factories;
    using RestSharp;

    /// <summary>
    /// Implements <see cref="IEntityAdapterClientFactory" />.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class EntityAdapterClientFactory : IEntityAdapterClientFactory
    {
        private readonly IEntityAdapterSettingsProvider settingsProvider;
        private readonly ILoggerWrapper loggerWrapper;
        private readonly ISpiExecutionContextManager spiExecutionContextManager;

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="EntityAdapterClientFactory" /> class.
        /// </summary>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        /// <param name="spiExecutionContextManager">
        /// An instance of type <see cref="ISpiExecutionContextManager" />.
        /// </param>
        public EntityAdapterClientFactory(
            IEntityAdapterSettingsProvider settingsProvider,
            ILoggerWrapper loggerWrapper,
            ISpiExecutionContextManager spiExecutionContextManager)
        {
            this.settingsProvider = settingsProvider;
            this.loggerWrapper = loggerWrapper;
            this.spiExecutionContextManager = spiExecutionContextManager;
        }

        /// <inheritdoc />
        public IEntityAdapterClient Create(
            string entityAdapterName,
            Uri baseUrl,
            Dictionary<string, string> headers)
        {
            // TODO: Make this defunct
            if (baseUrl != null || headers != null)
            {
                return CreateClient(entityAdapterName, baseUrl, headers);
            }

            return Create(entityAdapterName);
        }

        /// <inheritdoc />
        public IEntityAdapterClient Create(string entityAdapterName)
        {
            var headers = new Dictionary<string, string>
            {
                {"Ocp-Apim-Subscription-Key", this.settingsProvider.SubscriptionKey},
            };
            var baseUrl = new Uri(GetAdapterBaseUrl(entityAdapterName));

            return CreateClient(entityAdapterName, baseUrl, headers);
        }


        private string GetAdapterBaseUrl(string entityAdapterName)
        {
            if (string.IsNullOrEmpty(entityAdapterName))
            {
                throw new ArgumentNullException(nameof(entityAdapterName));
            }

            if (entityAdapterName.Equals(SourceSystemNames.GetInformationAboutSchools, StringComparison.InvariantCultureIgnoreCase))
            {
                return settingsProvider.GiasBaseUrl;
            }

            if (entityAdapterName.Equals(SourceSystemNames.UkRegisterOfLearningProviders, StringComparison.InvariantCultureIgnoreCase))
            {
                return settingsProvider.UkrlpBaseUrl;
            }

            if (entityAdapterName.Equals(SourceSystemNames.Rates, StringComparison.InvariantCultureIgnoreCase))
            {
                return settingsProvider.RatesBaseUrl;
            }

            if (entityAdapterName.Equals(SourceSystemNames.IStore, StringComparison.InvariantCultureIgnoreCase))
            {
                return settingsProvider.IStoreBaseUrl;
            }
            
            throw new ArgumentOutOfRangeException($"Unrecognised adapter {entityAdapterName}");
        }
        private IEntityAdapterClient CreateClient(
            string entityAdapterName,
            Uri baseUrl,
            Dictionary<string, string> headers)
        {
            EntityAdapterClient toReturn = null;

            RestClient restClient = new RestClient(baseUrl);

            if (headers != null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    restClient.AddDefaultHeader(header.Key, header.Value);
                }
            }

            toReturn = new EntityAdapterClient(
                this.loggerWrapper,
                restClient,
                this.spiExecutionContextManager,
                entityAdapterName);

            return toReturn;
        }
    }
}