using System.Net;
using Dfe.Spi.EntitySquasher.Domain;
using Dfe.Spi.EntitySquasher.Domain.Definitions;
using Dfe.Spi.EntitySquasher.Domain.Definitions.Factories;

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
        /// <param name="entityAdapterClientFactory">
        /// An instance of type <see cref="IEntityAdapterClientFactory" />.
        /// </param>
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

            var algorithm = this.CheckForDefaultAlgorithm(getSquashedEntityRequest.Algorithm);
            var fields = getSquashedEntityRequest.Fields != null
                ? getSquashedEntityRequest.Fields.ToArray()
                : null;

            var squashedEntityResults = await GetSquashedEntitiesAsync(
                algorithm,
                getSquashedEntityRequest.EntityName,
                getSquashedEntityRequest.EntityReferences.ToArray(),
                fields,
                getSquashedEntityRequest.AggregatesRequest,
                getSquashedEntityRequest.Live,
                cancellationToken);

            toReturn = new GetSquashedEntityResponse()
            {
                SquashedEntityResults = squashedEntityResults,
            };

            return toReturn;
        }

        private async Task<SquashedEntityResult[]> GetSquashedEntitiesAsync(
            string algorithm,
            string entityName,
            EntityReference[] entityReferences,
            string[] fields,
            AggregatesRequest aggregatesRequest,
            bool live,
            CancellationToken cancellationToken)
        {
            var adapterData = await this.entityAdapterInvoker.GetResultsFromAdaptersAsync(
                entityName, entityReferences, fields, aggregatesRequest, live, cancellationToken);

            var squashed = new SquashedEntityResult[entityReferences.Length];

            for (var i = 0; i < entityReferences.Length; i++)
            {
                var entityReference = entityReferences[i];
                var squashableEntities = entityReference.AdapterRecordReferences
                    .Select(x => adapterData[x])
                    .Where(x => x.EntityBase != null)
                    .ToArray();
                var squashedEntity = squashableEntities.Length > 0
                    ? await this.resultSquasher.SquashAsync(
                        algorithm,
                        entityName,
                        squashableEntities,
                        aggregatesRequest,
                        cancellationToken)
                    : null;

                var errors = entityReference.AdapterRecordReferences
                    .Select(x => new
                    {
                        AdapterReference = x,
                        AdapterException = adapterData[x].EntityAdapterException,
                    })
                    .Where(x => x.AdapterException != null)
                    .Select(x => new EntityAdapterErrorDetail
                    {
                        AdapterName = x.AdapterReference.Source,
                        RequestedFields = fields,
                        RequestedId = x.AdapterReference.Id,
                        RequestedEntityName = entityName,
                        HttpStatusCode = x.AdapterException.HttpStatusCode,
                        HttpErrorBody = x.AdapterException.HttpErrorBody,
                    });

                squashed[i] = new SquashedEntityResult
                {
                    EntityReference = entityReference,
                    SquashedEntity = squashedEntity,
                    EntityAdapterErrorDetails = errors,
                };
            }

            return squashed;
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