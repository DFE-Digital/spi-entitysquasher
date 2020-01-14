namespace Dfe.Spi.EntitySquasher.Application
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Managers;
    using Dfe.Spi.EntitySquasher.Application.Models.Result;
    using Dfe.Spi.EntitySquasher.Domain.Models.Acdf;

    /// <summary>
    /// Implements <see cref="IResultSquasher" />.
    /// </summary>
    public class ResultSquasher : IResultSquasher
    {
        private readonly IAlgorithmConfigurationDeclarationFileManager algorithmConfigurationDeclarationFileManager;
        private readonly ILoggerWrapper loggerWrapper;

        /// <summary>
        /// Initialises a new instance of the <see cref="ResultSquasher" />
        /// class.
        /// </summary>
        /// <param name="algorithmConfigurationDeclarationFileManager">
        /// An instance of type
        /// <see cref="IAlgorithmConfigurationDeclarationFileManager" />.
        /// </param>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        public ResultSquasher(
            IAlgorithmConfigurationDeclarationFileManager algorithmConfigurationDeclarationFileManager,
            ILoggerWrapper loggerWrapper)
        {
            this.algorithmConfigurationDeclarationFileManager = algorithmConfigurationDeclarationFileManager;
            this.loggerWrapper = loggerWrapper;
        }

        /// <inheritdoc />
        public async Task<Spi.Models.ModelsBase> SquashAsync(
            string algorithm,
            string entityName,
            IEnumerable<GetEntityAsyncResult> toSquash)
        {
            Spi.Models.ModelsBase toReturn = null;

            this.loggerWrapper.Debug(
                $"Pulling back " +
                $"{nameof(AlgorithmConfigurationDeclarationFile)} for " +
                $"{nameof(algorithm)} = \"{algorithm}\"...");

            // algorithmConfigurationDeclarationFile will always get populated
            // here, or throw an exception back up (FileNotFound).
            AlgorithmConfigurationDeclarationFile algorithmConfigurationDeclarationFile =
                await this.algorithmConfigurationDeclarationFileManager.GetAsync(
                    algorithm)
                    .ConfigureAwait(false);

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
            Type modelsBaseType = typeof(Spi.Models.ModelsBase);

            string typeNamespace = modelsBaseType.FullName.Replace(
                modelsBaseType.Name,
                entityName);

            this.loggerWrapper.Debug(
                $"{nameof(typeNamespace)} = \"{typeNamespace}\"");

            Assembly modelsAssembly = modelsBaseType.Assembly;

            toReturn = (Spi.Models.ModelsBase)modelsAssembly.CreateInstance(
                typeNamespace);

            this.loggerWrapper.Info(
                $"Empty instance of \"{entityName}\" created with success: " +
                $"{toReturn}.");

            // Now (attempt to) populate each property in turn.
            Type typeToReturn = modelsAssembly.GetType(typeNamespace);

            PropertyInfo[] propertiesToPopulate = typeToReturn.GetProperties();

            this.loggerWrapper.Debug(
                $"Cycling through {entityName}'s " +
                $"{propertiesToPopulate.Length} properties...");

            foreach (PropertyInfo propertyToPopulate in propertiesToPopulate)
            {
                this.PopulateProperty(
                    toReturn,
                    propertyToPopulate,
                    toSquash,
                    entity);
            }

            this.loggerWrapper.Info(
                $"Completed cycling through {entityName}'s " +
                $"{propertiesToPopulate.Length} properties.");

            return toReturn;
        }

        private void PopulateProperty(
            Spi.Models.ModelsBase modelsBase,
            PropertyInfo propertyToPopulate,
            IEnumerable<GetEntityAsyncResult> toSquash,
            Entity entity)
        {
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

            string name = propertyToPopulate.Name;

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
                string value = null;

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
                        // TODO: Handle different data types?
                        //       It's not likely everything will be a string...
                        object valueUnboxed = propertyToPopulate.GetValue(
                            getEntityAsyncResult.ModelsBase);

                        if (valueUnboxed != null)
                        {
                            value = valueUnboxed.ToString();
                        }

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
                    $"{nameof(value)} = \"{value}\". Updating property " +
                    $"\"{name}\" on our result model using reflection...");

                // value now has the value we need...
                // So use reflection to update our squashed model...
                propertyToPopulate.SetValue(modelsBase, value);

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
        }

        private bool IsFieldValueEmpty(
            Field field,
            string value)
        {
            bool toReturn = value == null;

            if (!toReturn && field.TreatWhitespaceAsNull)
            {
                toReturn = string.IsNullOrWhiteSpace(value);
            }

            return toReturn;
        }
    }
}