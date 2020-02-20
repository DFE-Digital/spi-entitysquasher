namespace Dfe.Spi.EntitySquasher.Application
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Caching.Definitions.Managers;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Factories;
    using Dfe.Spi.EntitySquasher.Application.Models.Result;
    using Dfe.Spi.EntitySquasher.Domain.Models.Acdf;
    using Dfe.Spi.Models;
    using Dfe.Spi.Models.Entities;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Implements <see cref="IResultSquasher" />.
    /// </summary>
    public class ResultSquasher : IResultSquasher
    {
        private static readonly Dictionary<Type, PropertyInfo[]> PropertyInfos =
            new Dictionary<Type, PropertyInfo[]>();

        private readonly ICacheManager cacheManager;
        private readonly ILoggerWrapper loggerWrapper;

        private readonly CamelCasePropertyNamesContractResolver camelCasePropertyNamesContractResolver;

        /// <summary>
        /// Initialises a new instance of the <see cref="ResultSquasher" />
        /// class.
        /// </summary>
        /// <param name="algorithmConfigurationDeclarationFileCacheManagerFactory">
        /// An instance of type
        /// <see cref="IAlgorithmConfigurationDeclarationFileCacheManagerFactory" />.
        /// </param>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        public ResultSquasher(
            IAlgorithmConfigurationDeclarationFileCacheManagerFactory algorithmConfigurationDeclarationFileCacheManagerFactory,
            ILoggerWrapper loggerWrapper)
        {
            if (algorithmConfigurationDeclarationFileCacheManagerFactory == null)
            {
                throw new ArgumentNullException(
                    nameof(algorithmConfigurationDeclarationFileCacheManagerFactory));
            }

            this.loggerWrapper = loggerWrapper;

            this.cacheManager =
                algorithmConfigurationDeclarationFileCacheManagerFactory.Create();

            this.camelCasePropertyNamesContractResolver =
                new CamelCasePropertyNamesContractResolver();
        }

        /// <inheritdoc />
        public async Task<EntityBase> SquashAsync(
            string algorithm,
            string entityName,
            IEnumerable<GetEntityAsyncResult> toSquash,
            CancellationToken cancellationToken)
        {
            EntityBase toReturn = null;

            this.loggerWrapper.Debug(
                $"Pulling back " +
                $"{nameof(AlgorithmConfigurationDeclarationFile)} for " +
                $"{nameof(algorithm)} = \"{algorithm}\"...");

            // algorithmConfigurationDeclarationFile will always get populated
            // here, or throw an exception back up (FileNotFound).
            object unboxedAlgorithmConfigurationDeclarationFile =
                await this.cacheManager.GetAsync(
                    algorithm,
                    cancellationToken)
                    .ConfigureAwait(false);

            AlgorithmConfigurationDeclarationFile algorithmConfigurationDeclarationFile =
                unboxedAlgorithmConfigurationDeclarationFile as AlgorithmConfigurationDeclarationFile;

            this.loggerWrapper.Info(
                $"{nameof(algorithm)} = \"{algorithm}\" returned: " +
                $"{algorithmConfigurationDeclarationFile}");

            this.loggerWrapper.Debug(
                $"Finding the entity configuration for \"{entityName}\"...");

            // Find the declared entity in the ACDF...
            Entity entity = algorithmConfigurationDeclarationFile.Entities
                .SingleOrDefault(x => x.Name == entityName);

            if (entity == null)
            {
                throw new InvalidAlgorithmConfigurationDeclarationFileException(
                    $"Unable to squash the results together, as the " +
                    $"specified {nameof(algorithm)}'s " +
                    $"{nameof(AlgorithmConfigurationDeclarationFile)} does " +
                    $"not specify configuration for the entity " +
                    $"\"{entityName}\"! An entry MUST exist for " +
                    $"\"{entityName}\".");
            }

            this.loggerWrapper.Info(
                $"{nameof(Entity)} found for \"{entityName}\": {entity}.");

            // Create a new instance of the requested model.
            Type entityBaseType = typeof(EntityBase);

            string typeNamespace = entityBaseType.FullName.Replace(
                entityBaseType.Name,
                entityName);

            this.loggerWrapper.Debug(
                $"{nameof(typeNamespace)} = \"{typeNamespace}\"");

            Assembly modelsAssembly = entityBaseType.Assembly;

            toReturn = (EntityBase)modelsAssembly.CreateInstance(
                typeNamespace);

            this.loggerWrapper.Info(
                $"Empty instance of \"{entityName}\" created with success: " +
                $"{toReturn}.");

            // Now (attempt to) populate each property in turn.
            Type typeToReturn = modelsAssembly.GetType(typeNamespace);

            // Little bit of caching.
            PropertyInfo[] propertiesToPopulate = null;
            if (PropertyInfos.ContainsKey(typeToReturn))
            {
                propertiesToPopulate = PropertyInfos[typeToReturn];
            }
            else
            {
                propertiesToPopulate = typeToReturn.GetProperties();

                PropertyInfos.Add(typeToReturn, propertiesToPopulate);
            }

            this.loggerWrapper.Debug(
                $"Cycling through {entityName}'s " +
                $"{propertiesToPopulate.Length} properties...");

            Dictionary<string, LineageEntry> lineage =
                new Dictionary<string, LineageEntry>();
            LineageEntry lineageEntry = null;
            foreach (PropertyInfo propertyToPopulate in propertiesToPopulate)
            {
                lineageEntry = this.PopulateProperty(
                    toReturn,
                    propertyToPopulate,
                    toSquash,
                    entity);

                if (lineageEntry != null)
                {
                    lineage.Add(propertyToPopulate.Name, lineageEntry);
                }
            }

            if (lineage.Count > 0)
            {
                toReturn._Lineage = lineage;
            }

            this.loggerWrapper.Info(
                $"Completed cycling through {entityName}'s " +
                $"{propertiesToPopulate.Length} properties.");

            return toReturn;
        }

        private LineageEntry PopulateProperty(
            EntityBase entityBase,
            PropertyInfo propertyToPopulate,
            IEnumerable<GetEntityAsyncResult> toSquash,
            Entity entity)
        {
            LineageEntry toReturn = null;

            string name = propertyToPopulate.Name;

            // Remember, EntityBases can be null if the adapter fails, and
            // _lineage will be null if it wasn't requested at the adapter
            // level.
            // So...
            // First get all the results where the calls were a success, and
            // where lineage is provided.
            IEnumerable<GetEntityAsyncResult> withSubLineage = toSquash
                .Where(x => x.EntityBase != null && x.EntityBase._Lineage != null);

            // We want to return null when no lineage has been provided.
            // So, let's see if we have any...
            if (withSubLineage.Any())
            {
                string nameCamelCase =
                    this.camelCasePropertyNamesContractResolver.GetResolvedPropertyName(name);

                IEnumerable<GetEntityAsyncResult> withAlternatviesInSubLineage =
                    withSubLineage
                        .Where(x => x.EntityBase._Lineage.ContainsKey(nameCamelCase));

                // We do... but... do we have lineage for *this property*?
                if (withAlternatviesInSubLineage.Any())
                {
                    // We do!
                    // So populate the alternatives with everything...
                    // Then we'll pick out the one that was chosen, and remove
                    // it from the alternatives down the road, and populate the
                    // top level with the actual chosen one.
                    LineageEntry[] lineageEntries =
                        withAlternatviesInSubLineage
                            .Select(x =>
                            {
                                LineageEntry lineageEntry =
                                    x.EntityBase._Lineage[nameCamelCase];

                                // The LineageEntry will come from the adapter
                                // with just a date, and not much else. It's up
                                // to us to flesh out the model a little.
                                lineageEntry.AdapterName =
                                    x.AdapterRecordReference.Source;

                                lineageEntry.Value = propertyToPopulate
                                    .GetValue(x.EntityBase);

                                return lineageEntry;
                            })
                            .ToArray();

                    toReturn = new LineageEntry()
                    {
                        Alternatives = lineageEntries,
                    };
                }
            }

            // Get an entity-level list of sources, if available.
            // This will either be null, or be populated.
            string[] sources = null;
            if (entity.Sources != null)
            {
                sources = entity.Sources.ToArray();
            }

            string entityName = entity.Name;

            string sourcesCsl = null;
            if (sources != null)
            {
                sourcesCsl = string.Join(", ", sources);
                this.loggerWrapper.Info(
                    $"{nameof(Entity)} level sources specified for " +
                    $"{entityName}: {sourcesCsl}.");
            }
            else
            {
                this.loggerWrapper.Debug(
                    $"No {nameof(Entity)} level sources specified for " +
                    $"{entityName}.");
            }

            this.loggerWrapper.Debug(
                $"Pulling back {nameof(Field)} configuration for " +
                $"\"{name}\"...");

            // Get the field declaration out of the entity.
            Field field = entity.Fields.SingleOrDefault(x => x.Name == name);

            if (field != null)
            {
                this.loggerWrapper.Info(
                    $"{nameof(Field)} configuration for \"{name}\": {field}.");

                // If we have sources listed at a property level, then
                // overwrite the sources with this.
                if (field.Sources != null)
                {
                    sources = field.Sources.ToArray();
                    sourcesCsl = string.Join(", ", sources);

                    this.loggerWrapper.Info(
                        $"{nameof(Entity)} level sources were specified, " +
                        $"but as were {nameof(Field)} level sources. The " +
                        $"{nameof(Field)} level sources will be used " +
                        $"instead: {sourcesCsl}.");
                }

                if (sources == null)
                {
                    throw new InvalidAlgorithmConfigurationDeclarationFileException(
                        $"No sources are specified, either at an " +
                        $"{nameof(Entity)} level, or a {nameof(Field)} " +
                        $"level. Without the list of sources, the squasher " +
                        $"cannot work how to compose the result. Sources " +
                        $"MUST be specified at either an {nameof(Entity)} " +
                        $"or {nameof(Field)} level (at minimum).");
                }

                // sources now contains the list we should use.
                // Let's go through in order.
                // *Only* keep looping whilst we *don't* have a value.
                object value = null;

                GetEntityAsyncResult getEntityAsyncResult = null;

                this.loggerWrapper.Debug(
                    $"Looping through all {nameof(sources.Length)} " +
                    $"source(s) until we have a result...");

                string currentSource = null;
                for (int i = 0; (i < sources.Length) && this.IsFieldValueEmpty(field, value); i++)
                {
                    // Get the next-most preferable source...
                    currentSource = sources[i];

                    // Get the corresponding GetEntityAsyncResult (if it
                    // exists)...
                    this.loggerWrapper.Debug(
                        $"Checking for result that came from " +
                        $"\"{currentSource}\"...");

                    getEntityAsyncResult = toSquash
                        .SingleOrDefault(x => x.AdapterRecordReference.Source == currentSource);

                    if (getEntityAsyncResult != null)
                    {
                        this.loggerWrapper.Info(
                            $"Result found for \"{currentSource}\": " +
                            $"{getEntityAsyncResult}.");

                        this.loggerWrapper.Debug(
                            $"Using reflection to get property value for " +
                            $"\"{name}\" on {getEntityAsyncResult}...");

                        // We have one!
                        // Now use reflection to get the correpsonding value.
                        value = propertyToPopulate.GetValue(
                            getEntityAsyncResult.EntityBase);

                        if (this.IsFieldValueEmpty(field, value))
                        {
                            this.loggerWrapper.Info(
                                $"Value for \"{name}\" on " +
                                $"{getEntityAsyncResult} is considered " +
                                $"empty. Looping will continue until a " +
                                $"result is found, or we run out of sources.");
                        }
                        else
                        {
                            this.loggerWrapper.Info(
                                $"Value for \"{name}\" is not considered " +
                                $"empty, and will be used to populate our " +
                                $"result model.");
                        }
                    }
                    else
                    {
                        this.loggerWrapper.Info(
                            $"\"{currentSource}\" was specified as a " +
                            $"preference, but this preference did not " +
                            $"exist in {nameof(toSquash)} to pick from.");
                    }
                }

                this.loggerWrapper.Debug(
                    $"{nameof(value)} = {value}. Updating property " +
                    $"\"{name}\" on our result model using reflection...");

                // value now has the value we need...
                // So use reflection to update our squashed model...
                propertyToPopulate.SetValue(entityBase, value);

                this.loggerWrapper.Info(
                    $"\"{name}\" was updated to \"{value}\" using " +
                    $"reflection.");
            }
            else
            {
                this.loggerWrapper.Info(
                    $"No field entry found in {entity}! This field will not " +
                    $"be populated.");
            }

            return toReturn;
        }

        private bool IsFieldValueEmpty(
            Field field,
            object value)
        {
            bool toReturn = value == null;

            string name = field.Name;
            if (!toReturn && field.TreatWhitespaceAsNull)
            {
                this.loggerWrapper.Debug(
                    $"Field \"{name}\" has been marked to treat whitespace " +
                    $"as null. We should ignore this option if the value " +
                    $"isn't a {nameof(String)} (otherwise, how could it be " +
                    $"whitespace?).");

                string valueStr = value as string;
                if (valueStr != null)
                {
                    toReturn = string.IsNullOrWhiteSpace(valueStr);

                    if (toReturn)
                    {
                        this.loggerWrapper.Debug(
                            $"Value for field \"{name}\" is empty or " +
                            $"whitespace.");
                    }
                }
                else
                {
                    this.loggerWrapper.Info(
                        $"Field \"{name}\" has been marked to treat " +
                        $"whitespace as null, but the value is not a " +
                        $"{nameof(String)}. This option will be ignored.");
                }
            }
            else
            {
                this.loggerWrapper.Debug(
                    $"Value for field \"{name}\" is null.");
            }

            return toReturn;
        }
    }
}