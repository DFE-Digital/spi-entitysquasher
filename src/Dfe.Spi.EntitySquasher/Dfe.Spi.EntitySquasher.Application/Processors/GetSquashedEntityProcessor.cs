namespace Dfe.Spi.EntitySquasher.Application.Processors
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions.SettingsProviders;
    using Dfe.Spi.EntitySquasher.Application.Models;
    using Dfe.Spi.EntitySquasher.Application.Processors.Definitions;

    /// <summary>
    /// Implements <see cref="IGetSquashedEntityProcessor" />.
    /// </summary>
    public class GetSquashedEntityProcessor : IGetSquashedEntityProcessor
    {
        private readonly IEntityAdapterInvoker entityAdapterInvoker;
        private readonly IGetSquashedEntityProcessorSettingsProvider getSquashedEntityProcessorSettingsProvider;
        private readonly ILoggerWrapper loggerWrapper;

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="GetSquashedEntityProcessor" /> class.
        /// </summary>
        /// <param name="entityAdapterInvoker">
        /// An instance of type <see cref="IEntityAdapterInvoker" />.
        /// </param>
        /// <param name="getSquashedEntityProcessorSettingsProvider">
        /// An instance of type
        /// <see cref="IGetSquashedEntityProcessorSettingsProvider" />.
        /// </param>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        public GetSquashedEntityProcessor(
            IEntityAdapterInvoker entityAdapterInvoker,
            IGetSquashedEntityProcessorSettingsProvider getSquashedEntityProcessorSettingsProvider,
            ILoggerWrapper loggerWrapper)
        {
            this.entityAdapterInvoker = entityAdapterInvoker;
            this.getSquashedEntityProcessorSettingsProvider = getSquashedEntityProcessorSettingsProvider;
            this.loggerWrapper = loggerWrapper;
        }

        /// <inheritdoc />
        public async Task<GetSquashedEntityResponse> GetSquashedEntityAsync(
            GetSquashedEntityRequest getSquashedEntityRequest)
        {
            GetSquashedEntityResponse toReturn = null;

            if (getSquashedEntityRequest == null)
            {
                throw new ArgumentNullException(
                    nameof(getSquashedEntityRequest));
            }

            string algorithm = getSquashedEntityRequest.Algorithm;
            algorithm = this.CheckForDefaultAlgorithm(algorithm);

            string entityName = getSquashedEntityRequest.EntityName;
            IEnumerable<string> fields = getSquashedEntityRequest.Fields;

            IEnumerable<EntityReference> entityReferences =
                getSquashedEntityRequest.EntityReferences;

            List<SquashedEntityResult> squashedEntityResults =
                new List<SquashedEntityResult>();

            // It would probably be sensible to populate each entity in turn,
            // rather than in parallel.
            // We can call the adapters in parallel, if we have lots in this
            // array, we don't want to bombard the adapters too much.
            SquashedEntityResult squashedEntityResult = null;
            foreach (EntityReference entityReference in entityReferences)
            {
                // This can be done with LINQ, but looks messy AF with the
                // async stuff going on.
                squashedEntityResult =
                    await this.GetSquashedEntityResultAsync(
                        algorithm,
                        entityName,
                        fields,
                        entityReference)
                        .ConfigureAwait(false);

                squashedEntityResults.Add(squashedEntityResult);
            }

            toReturn = new GetSquashedEntityResponse()
            {
                SquashedEntityResults = squashedEntityResults,
            };

            return toReturn;
        }

        private async Task<SquashedEntityResult> GetSquashedEntityResultAsync(
            string algorithm,
            string entityName,
            IEnumerable<string> fields,
            EntityReference entityReference)
        {
            SquashedEntityResult toReturn = null;

            // 1) Call all adapters specified in the entity reference
            //    at the same time. First, get the tasks to pull back the
            //    ModelsBases.
            InvokeEntityAdaptersResult adaptersLookupResult =
                await this.entityAdapterInvoker.InvokeEntityAdapters(
                    algorithm,
                    entityName,
                    fields,
                    entityReference)
                    .ConfigureAwait(false);

            // TODO:
            // 2) Perform the squashing and append to the result.
            Spi.Models.ModelsBase squashedEntity = null;

            toReturn = new SquashedEntityResult()
            {
                EntityReference = entityReference,
                EntityAdapterExceptions = adaptersLookupResult.EntityAdapterExceptions,
                SquashedEntity = squashedEntity,
            };

            return toReturn;
        }

        private string CheckForDefaultAlgorithm(string algorithm)
        {
            string toReturn = algorithm;

            if (string.IsNullOrEmpty(toReturn))
            {
                this.loggerWrapper.Debug(
                    "No algorithm specified. The default algorithm will be " +
                    "used for this request.");

                toReturn = this.getSquashedEntityProcessorSettingsProvider
                    .DefaultAlgorithm;
            }

            this.loggerWrapper.Info($"{nameof(toReturn)} = \"{toReturn}\"");

            return toReturn;
        }
    }
}