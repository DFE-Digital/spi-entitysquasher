namespace Dfe.Spi.EntitySquasher.Application.Processors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Managers;
    using Dfe.Spi.EntitySquasher.Application.Definitions.SettingsProviders;
    using Dfe.Spi.EntitySquasher.Application.Models;
    using Dfe.Spi.EntitySquasher.Application.Processors.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Models.Acdf;

    /// <summary>
    /// Implements <see cref="IGetSquashedEntityProcessor" />.
    /// </summary>
    public class GetSquashedEntityProcessor : IGetSquashedEntityProcessor
    {
        private readonly IAlgorithmConfigurationDeclarationFileManager algorithmConfigurationDeclarationFileManager;
        private readonly IEntityAdapterClientManager entityAdapterClientManager;
        private readonly IGetSquashedEntityProcessorSettingsProvider getSquashedEntityProcessorSettingsProvider;
        private readonly ILoggerWrapper loggerWrapper;

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="GetSquashedEntityProcessor" /> class.
        /// </summary>
        /// <param name="algorithmConfigurationDeclarationFileManager">
        /// An instance of type
        /// <see cref="IAlgorithmConfigurationDeclarationFileManager" />.
        /// </param>
        /// <param name="entityAdapterClientManager">
        /// An instance of type <see cref="IEntityAdapterClientManager" />.
        /// </param>
        /// <param name="getSquashedEntityProcessorSettingsProvider">
        /// An instance of type
        /// <see cref="IGetSquashedEntityProcessorSettingsProvider" />.
        /// </param>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        public GetSquashedEntityProcessor(
            IAlgorithmConfigurationDeclarationFileManager algorithmConfigurationDeclarationFileManager,
            IEntityAdapterClientManager entityAdapterClientManager,
            IGetSquashedEntityProcessorSettingsProvider getSquashedEntityProcessorSettingsProvider,
            ILoggerWrapper loggerWrapper)
        {
            this.algorithmConfigurationDeclarationFileManager = algorithmConfigurationDeclarationFileManager;
            this.entityAdapterClientManager = entityAdapterClientManager;
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

            this.loggerWrapper.Debug(
                $"Fetching " +
                $"{nameof(AlgorithmConfigurationDeclarationFile)}...");

            // TODO: Are we even using this? Remove at some point? Can leave
            //       in for now, I guess.
            AlgorithmConfigurationDeclarationFile algorithmConfigurationDeclarationFile =
                await this.algorithmConfigurationDeclarationFileManager.GetAsync(
                    algorithm)
                    .ConfigureAwait(false);

            this.loggerWrapper.Info(
                $"{nameof(algorithmConfigurationDeclarationFile)} = " +
                $"{algorithmConfigurationDeclarationFile}");

            // TODO:
            // 1) Pull from the requested adapters and;
            // 2) Squash the entities together according to the rules outlined
            //    in the ACDF.
            string entityName = getSquashedEntityRequest.EntityName;
            IEnumerable<string> fields = getSquashedEntityRequest.Fields;

            IEnumerable<EntityReference> entityReferences =
                getSquashedEntityRequest.EntityReferences;

            List<Spi.Models.ModelsBase> entities =
                new List<Spi.Models.ModelsBase>();

            // It would probably be sensible to populate each entity in turn,
            // rather than in parallel.
            // We can call the adapters in parallel, if we have lots in this
            // array, we don't want to bombard the adapters too much.
            Spi.Models.ModelsBase modelsBase = null;
            foreach (EntityReference entityReference in entityReferences)
            {
                // This can be done with LINQ, but looks messy AF with the
                // async stuff going on.
                modelsBase = await this.ProcessSingleEntityReferenceAsync(
                    algorithm,
                    entityName,
                    fields,
                    entityReference)
                    .ConfigureAwait(false);

                entities.Add(modelsBase);
            }

            toReturn = new GetSquashedEntityResponse()
            {
                Entities = entities,
            };

            return toReturn;
        }

        private async Task<Spi.Models.ModelsBase> ProcessSingleEntityReferenceAsync(
            string algorithm,
            string entityName,
            IEnumerable<string> fields,
            EntityReference entityReference)
        {
            Spi.Models.ModelsBase toReturn = null;

            // 1) Call all adapters specified in the entity reference
            //    at the same time. First, get the tasks to pull back the
            //    ModelsBases.
            IEnumerable<AdapterRecordReference> adapterRecordReferences =
                entityReference.AdapterRecordReferences;

            List<Task<Spi.Models.ModelsBase>> fetchTasks =
                new List<Task<Spi.Models.ModelsBase>>();

            Task<Spi.Models.ModelsBase> task = null;
            foreach (AdapterRecordReference adapterRecordReference in adapterRecordReferences)
            {
                task = await this.GetEntityAsyncTaskAsync(
                    algorithm,
                    entityName,
                    fields,
                    adapterRecordReference)
                    .ConfigureAwait(false);
            }

            // Then, execute them all and wait for the pulling back of all
            // tasks to complete, in parallel (so basically, yeild).
            await Task.WhenAll(fetchTasks).ConfigureAwait(false);

            // We should have the results now.
            IEnumerable<Spi.Models.ModelsBase> models = fetchTasks
                .Select(x => x.Result);

            // TODO:
            // 2) Perform the squashing.
            return toReturn;
        }

        private async Task<Task<Spi.Models.ModelsBase>> GetEntityAsyncTaskAsync(
            string algorithm,
            string entityName,
            IEnumerable<string> fields,
            AdapterRecordReference adapterRecordReference)
        {
            Task<Spi.Models.ModelsBase> toReturn = null;

            string source = adapterRecordReference.Source;

            EntityAdapterClientKey entityAdapterClientKey =
                new EntityAdapterClientKey()
                {
                    Algorithm = algorithm,
                    Name = source,
                };

            // Get the entity adapter client from the manager and...
            IEntityAdapterClient entityAdapterClient =
                await this.entityAdapterClientManager.GetAsync(
                    entityAdapterClientKey)
                    .ConfigureAwait(false);

            // Get the task, and return it.
            string id = adapterRecordReference.Id;

            toReturn = entityAdapterClient.GetEntityAsync(
                entityName,
                id,
                fields);

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