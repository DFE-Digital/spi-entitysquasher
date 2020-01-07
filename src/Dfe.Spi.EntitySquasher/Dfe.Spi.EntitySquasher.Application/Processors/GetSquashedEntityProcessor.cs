namespace Dfe.Spi.EntitySquasher.Application.Processors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Http.Client;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Managers;
    using Dfe.Spi.EntitySquasher.Application.Definitions.SettingsProviders;
    using Dfe.Spi.EntitySquasher.Application.Models;
    using Dfe.Spi.EntitySquasher.Application.Processors.Definitions;
    using Dfe.Spi.EntitySquasher.Domain;
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

            // TODO: This class is getting rather large.
            //       Look at refactoring it.
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
            AdaptersLookupResult adaptersLookupResult =
                await this.GetAdaptersLookupResultAsync(
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

        private async Task<AdaptersLookupResult> GetAdaptersLookupResultAsync(
            string algorithm,
            string entityName,
            IEnumerable<string> fields,
            EntityReference entityReference)
        {
            AdaptersLookupResult toReturn = null;

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

                fetchTasks.Add(task);
            }

            try
            {
                // Then, execute them all and wait for the pulling back of all
                // tasks to complete, in parallel (so basically, yeild).
                await Task.WhenAll(fetchTasks).ConfigureAwait(false);
            }
            catch (EntityAdapterException entityAdapterException)
            {
                // Note: This try-catch will only throw back the *first*
                //       exception that occurrs on the pool of tasks.
                this.loggerWrapper.Warning(
                    $"An adapter threw a {nameof(EntityAdapterException)}. " +
                    $"Note that this is only the first exception thrown - " +
                    $"the underlying tasks will be checked for thrown " +
                    $"exceptions.",
                    entityAdapterException);
            }

            // We should have the results now.
            IEnumerable<Spi.Models.ModelsBase> modelsBases = fetchTasks
                .Where(x => x.Status == TaskStatus.RanToCompletion)
                .Select(x => x.Result);

            this.loggerWrapper.Debug(
                $"Number of models (successfully) returned: " +
                $"{modelsBases.Count()}.");

            // Check for exceptions, too.
            // Now, all exceptions *should* be SpiWebServiceExceptions, if
            // it's running in an *handled* way. If there are exceptions
            // that are not SpiWebServiceExceptions, we need to re-throw them,
            // as this shouldn't be happening.
            IEnumerable<Exception> taskExceptions = fetchTasks
                .Where(x => x.Status == TaskStatus.Faulted)
                .SelectMany(x => x.Exception.InnerExceptions);

            IEnumerable<EntityAdapterException> entityAdapterExceptions =
                taskExceptions
                    .Where(x => x is EntityAdapterException)
                    .Cast<EntityAdapterException>();

            if (taskExceptions.Count() != entityAdapterExceptions.Count())
            {
                throw new AggregateException(
                    $"Some exceptions thrown by one or more of the adapters " +
                    $"were not {nameof(SpiWebServiceException)}s! These " +
                    $"are currently not handled. Please investigate.",
                    taskExceptions);
            }

            this.loggerWrapper.Debug(
                $"Number of faulted/handled tasks: " +
                $"{entityAdapterExceptions.Count()}.");

            if (!modelsBases.Any())
            {
                // We've got nowt to squash!
                throw new AllAdaptersUnavailableException(
                    entityAdapterExceptions);
            }

            toReturn = new AdaptersLookupResult()
            {
                ModelsBases = modelsBases,
                EntityAdapterExceptions = entityAdapterExceptions,
            };

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