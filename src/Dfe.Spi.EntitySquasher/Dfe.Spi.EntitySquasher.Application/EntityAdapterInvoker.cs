namespace Dfe.Spi.EntitySquasher.Application
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Managers;
    using Dfe.Spi.EntitySquasher.Application.Models;
    using Dfe.Spi.EntitySquasher.Domain;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;

    /// <summary>
    /// Implements <see cref="IEntityAdapterInvoker" />.
    /// </summary>
    public class EntityAdapterInvoker : IEntityAdapterInvoker
    {
        private readonly IEntityAdapterClientManager entityAdapterClientManager;
        private readonly ILoggerWrapper loggerWrapper;

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="EntityAdapterInvoker" /> class.
        /// </summary>
        /// <param name="entityAdapterClientManager">
        /// An instance of type <see cref="IEntityAdapterClientManager" />.
        /// </param>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        public EntityAdapterInvoker(
            IEntityAdapterClientManager entityAdapterClientManager,
            ILoggerWrapper loggerWrapper)
        {
            this.entityAdapterClientManager = entityAdapterClientManager;
            this.loggerWrapper = loggerWrapper;
        }

        /// <inheritdoc />
        public async Task<InvokeEntityAdaptersResult> InvokeEntityAdapters(
            string algorithm,
            string entityName,
            IEnumerable<string> fields,
            EntityReference entityReference)
        {
            InvokeEntityAdaptersResult toReturn = null;

            if (entityReference == null)
            {
                throw new ArgumentNullException(nameof(entityReference));
            }

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
                    $"were not {nameof(EntityAdapterException)}s! These " +
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

            toReturn = new InvokeEntityAdaptersResult()
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
    }
}