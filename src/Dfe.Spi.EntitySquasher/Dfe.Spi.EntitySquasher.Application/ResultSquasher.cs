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

            AlgorithmConfigurationDeclarationFile algorithmConfigurationDeclarationFile =
                await this.algorithmConfigurationDeclarationFileManager.GetAsync(
                    algorithm)
                    .ConfigureAwait(false);

            // Find the declared entity in the ACDF...
            Entity entity = algorithmConfigurationDeclarationFile.Entities
                .Single(x => x.Name == entityName);

            // Create a new instance of the requested model.
            Type modelsBaseType = typeof(Spi.Models.ModelsBase);

            string typeNamespace = modelsBaseType.FullName.Replace(
                modelsBaseType.Name,
                entityName);

            Assembly modelsAssembly = modelsBaseType.Assembly;

            toReturn = (Spi.Models.ModelsBase)modelsAssembly.CreateInstance(
                typeNamespace);

            // Now (attempt to) populate each property in turn.
            Type typeToReturn = modelsAssembly.GetType(typeNamespace);

            PropertyInfo[] propertiesToPopulate = typeToReturn.GetProperties();
            foreach (PropertyInfo propertyToPopulate in propertiesToPopulate)
            {
                this.PopulateProperty(
                    toReturn,
                    propertyToPopulate,
                    toSquash,
                    entity);
            }

            return toReturn;
        }

        private void PopulateProperty(
            Spi.Models.ModelsBase modelsBase,
            PropertyInfo propertyToPopulate,
            IEnumerable<GetEntityAsyncResult> toSquash,
            Entity entity)
        {
            string name = propertyToPopulate.Name;

            // Get an entity-level list of sources, if available.
            // This will either be null, or be populated.
            string[] sources = entity.Sources.ToArray();

            // Get the field declaration out of the entity.
            Field field = entity.Fields.SingleOrDefault(x => x.Name == name);

            if (field != null)
            {
                // If we have sources listed at a property level, then
                // overwrite the sources with this.
                if (field.Sources != null)
                {
                    sources = field.Sources.ToArray();
                }

                // sources now contains the list we should use.
                // Let's go through in order.
                // *Only* keep looping whilst we *don't* have a value.
                string value = null;

                GetEntityAsyncResult getEntityAsyncResult = null;

                string currentSource = null;
                for (int i = 0; (i < sources.Length) && string.IsNullOrEmpty(value); i++)
                {
                    // Get the next-most preferable source...
                    currentSource = sources[i];

                    // Get the corresponding GetEntityAsyncResult (if it
                    // exists)...
                    getEntityAsyncResult = toSquash
                        .SingleOrDefault(x => x.AdapterRecordReference.Source == currentSource);

                    if (getEntityAsyncResult != null)
                    {
                        // We have one!
                        // Now use reflection to get the correpsonding value.
                        value = propertyToPopulate.GetValue(
                            getEntityAsyncResult.ModelsBase).ToString();
                    }
                    else
                    {
                        this.loggerWrapper.Info(
                            $"\"{currentSource}\" was specified as a " +
                            $"preference, but this preference did not " +
                            $"exist in {nameof(toSquash)} to pick from.");
                    }
                }

                // value now has the value we need...
                // So use reflection to update our squashed model...
                propertyToPopulate.SetValue(modelsBase, value);
            }
            else
            {
                this.loggerWrapper.Info(
                    $"No field entry found in {entity}! This field will not " +
                    $"be populated.");
            }
        }
    }
}