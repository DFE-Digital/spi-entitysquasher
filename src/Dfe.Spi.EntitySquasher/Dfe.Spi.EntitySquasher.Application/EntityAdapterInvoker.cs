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

            List<GetEntityAsyncTaskContainer> fetchTasks =
                new List<GetEntityAsyncTaskContainer>();

            this.loggerWrapper.Debug(
                $"Fetching from the appropriate adapters the " +
                $"{nameof(Task)}s to fetch the {nameof(ModelsBase)}s...");

            Task<Spi.Models.ModelsBase> task = null;
            GetEntityAsyncTaskContainer getEntityTaskContainer = null;
            foreach (AdapterRecordReference adapterRecordReference in adapterRecordReferences)
            {
                task = await this.GetEntityAsyncTaskAsync(
                    algorithm,
                    entityName,
                    fields,
                    adapterRecordReference)
                    .ConfigureAwait(false);

                getEntityTaskContainer = new GetEntityAsyncTaskContainer()
                {
                    AdapterRecordReference = adapterRecordReference,
                    Task = task,
                };

                fetchTasks.Add(getEntityTaskContainer);
            }

            this.loggerWrapper.Info(
                $"{fetchTasks.Count} {nameof(Task)}(s) returned. Waiting " +
                $"for them to come to an end...");

            try
            {
                // Then, execute them all and wait for the pulling back of all
                // tasks to complete, in parallel (so basically, yeild).
                await Task.WhenAll(fetchTasks.Select(x => x.Task))
                    .ConfigureAwait(false);
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

            this.loggerWrapper.Debug(
                $"All {nameof(Task)}s have finished. Checking " +
                $"{nameof(Task)} collection for results...");

            // We should have the results now.
            IEnumerable<GetEntityAsyncResult> successfulTasks = fetchTasks
                .Where(x => x.Task.Status == TaskStatus.RanToCompletion)
                .Select(x => new GetEntityAsyncResult()
                {
                    AdapterRecordReference = x.AdapterRecordReference,
                    ModelsBase = x.Task.Result,
                });

            this.loggerWrapper.Debug(
                $"Number of {nameof(ModelsBase)}s (successfully) returned: " +
                $"{successfulTasks.Count()}.");

            AssertExceptionsAreHandled(fetchTasks);

            IEnumerable<GetEntityAsyncResult> failedTasks = fetchTasks
                .Where(x => x.Task.Status == TaskStatus.Faulted)
                .Select(x => new GetEntityAsyncResult()
                {
                    AdapterRecordReference = x.AdapterRecordReference,
                    EntityAdapterException = (EntityAdapterException)x.Task.Exception.InnerException,
                });

            this.loggerWrapper.Debug(
                $"Number of errored (yet handled) tasks: " +
                $"{failedTasks.Count()}");

            if (!successfulTasks.Any())
            {
                IEnumerable<EntityAdapterException> entityAdapterExceptions =
                    failedTasks.Select(x => x.EntityAdapterException);

                // This means all the tasks failed -
                // We've got nowt to squash!
                throw new AllAdaptersUnavailableException(
                    entityAdapterExceptions);
            }

            IEnumerable<GetEntityAsyncResult> getEntityAsyncResults =
                successfulTasks.Concat(failedTasks);

            toReturn = new InvokeEntityAdaptersResult()
            {
                GetEntityAsyncResults = getEntityAsyncResults,
            };

            return toReturn;
        }

        private static void AssertExceptionsAreHandled(
            IEnumerable<GetEntityAsyncTaskContainer> fetchTasks)
        {
            // Check for exceptions, too.
            // Now, all exceptions *should* be EntityAdapterExceptions, if
            // it's running in an *handled* way. If there are exceptions
            // that are not EntityAdapterExceptions, we need to re-throw them,
            // as this shouldn't be happening.
            IEnumerable<Exception> taskExceptions = fetchTasks
                .Where(x => x.Task.Status == TaskStatus.Faulted)
                .Select(x => x.Task.Exception.InnerException);

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

            this.loggerWrapper.Debug(
                $"Fetching {nameof(IEntityAdapterClient)} for " +
                $"{entityAdapterClientKey}...");

            // Get the entity adapter client from the manager and...
            IEntityAdapterClient entityAdapterClient =
                await this.entityAdapterClientManager.GetAsync(
                    entityAdapterClientKey)
                    .ConfigureAwait(false);

            this.loggerWrapper.Info(
                $"{nameof(IEntityAdapterClient)} returned for " +
                $"{entityAdapterClientKey}.");

            // Get the task, and return it.
            string id = adapterRecordReference.Id;

            this.loggerWrapper.Debug(
                $"Getting {nameof(Task)} for " +
                $"{nameof(IEntityAdapterClient.GetEntityAsync)} call...");

            toReturn = entityAdapterClient.GetEntityAsync(
                entityName,
                id,
                fields);

            this.loggerWrapper.Info($"{nameof(Task)} obtained. Returning...");

            return toReturn;
        }
    }
}