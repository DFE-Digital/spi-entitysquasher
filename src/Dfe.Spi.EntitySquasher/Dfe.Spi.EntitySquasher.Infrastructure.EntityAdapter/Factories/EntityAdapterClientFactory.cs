namespace Dfe.Spi.EntitySquasher.Infrastructure.EntityAdapter.Factories
{
    using System;
    using System.Diagnostics.CodeAnalysis;
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

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="EntityAdapterClientFactory" /> class.
        /// </summary>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        public EntityAdapterClientFactory(ILoggerWrapper loggerWrapper)
        {
            this.loggerWrapper = loggerWrapper;
        }

        /// <inheritdoc />
        public IEntityAdapterClient Create(Uri baseUrl)
        {
            EntityAdapterClient toReturn = null;

            RestClient restClient = new RestClient(baseUrl);

            toReturn = new EntityAdapterClient(
                this.loggerWrapper,
                restClient);

            return toReturn;
        }
    }
}