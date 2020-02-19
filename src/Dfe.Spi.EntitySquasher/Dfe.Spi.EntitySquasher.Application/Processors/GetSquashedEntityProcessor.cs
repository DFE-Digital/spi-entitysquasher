namespace Dfe.Spi.EntitySquasher.Application.Processors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions.SettingsProviders;
    using Dfe.Spi.EntitySquasher.Application.Models;
    using Dfe.Spi.EntitySquasher.Application.Models.Processors;
    using Dfe.Spi.EntitySquasher.Application.Models.Result;
    using Dfe.Spi.EntitySquasher.Application.Processors.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Models;
    using Dfe.Spi.Models.Entities;

    /// <summary>
    /// Implements <see cref="IGetSquashedEntityProcessor" />.
    /// </summary>
    public class GetSquashedEntityProcessor : IGetSquashedEntityProcessor
    {
        private readonly IEntityAdapterInvoker entityAdapterInvoker;
        private readonly IGetSquashedEntityProcessorSettingsProvider getSquashedEntityProcessorSettingsProvider;
        private readonly ILoggerWrapper loggerWrapper;
        private readonly IResultSquasher resultSquasher;

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
        /// <param name="resultSquasher">
        /// An instance of type <see cref="IResultSquasher" />.
        /// </param>
        public GetSquashedEntityProcessor(
            IEntityAdapterInvoker entityAdapterInvoker,
            IGetSquashedEntityProcessorSettingsProvider getSquashedEntityProcessorSettingsProvider,
            ILoggerWrapper loggerWrapper,
            IResultSquasher resultSquasher)
        {
            this.entityAdapterInvoker = entityAdapterInvoker;
            this.getSquashedEntityProcessorSettingsProvider = getSquashedEntityProcessorSettingsProvider;
            this.loggerWrapper = loggerWrapper;
            this.resultSquasher = resultSquasher;
        }

        /// <inheritdoc />
        public async Task<GetSquashedEntityResponse> GetSquashedEntityAsync(
            GetSquashedEntityRequest getSquashedEntityRequest,
            CancellationToken cancellationToken)
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
                        entityReference,
                        cancellationToken)
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
            EntityReference entityReference,
            CancellationToken cancellationToken)
        {
            SquashedEntityResult toReturn = null;

            // 1) Call all adapters specified in the entity reference
            //    at the same time. First, get the tasks to pull back the
            //    EntityBase.
            IEnumerable<EntityAdapterErrorDetail> entityAdapterErrorDetails = null;
            EntityBase squashedEntity = null;
            try
            {
                InvokeEntityAdaptersResult adaptersLookupResult =
                    await this.entityAdapterInvoker.InvokeEntityAdaptersAsync(
                        algorithm,
                        entityName,
                        fields,
                        entityReference,
                        cancellationToken)
                        .ConfigureAwait(false);

                IEnumerable<GetEntityAsyncResult> getEntityAsyncResults =
                    adaptersLookupResult.GetEntityAsyncResults;

                entityAdapterErrorDetails =
                    getEntityAsyncResults
                        .Where(x => x.EntityAdapterException != null)
                        .Select(x => x.EntityAdapterException.EntityAdapterErrorDetail);

                // 2) Perform the squashing and append to the result - with
                //    *these* guys.
                IEnumerable<GetEntityAsyncResult> toSquash =
                    getEntityAsyncResults
                        .Where(x => x.EntityBase != null);

                squashedEntity =
                    await this.resultSquasher.SquashAsync(
                        algorithm,
                        entityName,
                        toSquash,
                        cancellationToken)
                        .ConfigureAwait(false);
            }
            catch (AllAdaptersUnavailableException allAdaptersUnavailableException)
            {
                this.loggerWrapper.Warning(
                    "Warning! All adapters were unavailable when attempting " +
                    "to serve this request. This is probably an indicator " +
                    "that something is very wrong! The request will still " +
                    "be served, but the model will be empty.",
                    allAdaptersUnavailableException);

                entityAdapterErrorDetails = allAdaptersUnavailableException
                    .EntityAdapterExceptions
                    .Select(x => x.EntityAdapterErrorDetail);
            }

            toReturn = new SquashedEntityResult()
            {
                EntityReference = entityReference,
                EntityAdapterErrorDetails = entityAdapterErrorDetails,
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