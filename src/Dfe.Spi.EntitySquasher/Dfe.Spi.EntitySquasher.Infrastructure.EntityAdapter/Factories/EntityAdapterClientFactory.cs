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
            ILoggerWrapper loggerWrapper,
            ISpiExecutionContextManager spiExecutionContextManager)
        {
            this.loggerWrapper = loggerWrapper;
            this.spiExecutionContextManager = spiExecutionContextManager;
        }

        /// <inheritdoc />
        public IEntityAdapterClient Create(
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