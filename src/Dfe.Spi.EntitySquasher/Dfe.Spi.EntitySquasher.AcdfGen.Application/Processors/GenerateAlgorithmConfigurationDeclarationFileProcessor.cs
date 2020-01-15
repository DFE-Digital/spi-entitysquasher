namespace Dfe.Spi.EntitySquasher.AcdfGen.Application.Processors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.AcdfGen.Application.Definitions.Processors;
    using Dfe.Spi.EntitySquasher.AcdfGen.Application.Models;
    using Dfe.Spi.EntitySquasher.Domain.Models.Acdf;

    /// <summary>
    /// Implements
    /// <see cref="IGenerateAlgorithmConfigurationDeclarationFileProcessor" />.
    /// </summary>
    public class GenerateAlgorithmConfigurationDeclarationFileProcessor :
        IGenerateAlgorithmConfigurationDeclarationFileProcessor
    {
        private readonly ILoggerWrapper loggerWrapper;

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="GenerateAlgorithmConfigurationDeclarationFileProcessor" />
        /// class.
        /// </summary>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        public GenerateAlgorithmConfigurationDeclarationFileProcessor(
            ILoggerWrapper loggerWrapper)
        {
            this.loggerWrapper = loggerWrapper;
        }

        /// <inheritdoc />
        public GenerateAlgorithmConfigurationDeclarationFileResponse GenerateAlgorithmConfigurationDeclarationFile(
            GenerateAlgorithmConfigurationDeclarationFileRequest generateAlgorithmConfigurationDeclarationFileRequest)
        {
            GenerateAlgorithmConfigurationDeclarationFileResponse toReturn = null;

            // 1) Use reflection to cycle through the concrete types available
            //    in Dfe.Spi.Models. Generate the majority of the file.
            IEnumerable<Entity> entities = this.GenerateEntities();

            // TODO: Temporary code to enumerate the collection. To remove.
            entities.ToArray();

            // TODO:
            // 2) Generate placeholders for the adapters based on the input.
            // 3) Save the file with a filename based on input.
            return toReturn;
        }

        private IEnumerable<Entity> GenerateEntities()
        {
            IEnumerable<Entity> toReturn = null;

            // Use ModelsBase as a means to getting a reference to the
            // assembly.
            Type modelsBaseType = typeof(Spi.Models.ModelsBase);

            Assembly assembly = modelsBaseType.Assembly;

            Type[] types = assembly.GetTypes();

            this.loggerWrapper.Debug(
                $"Enumerating non-abstract types from the " +
                $"{nameof(Assembly)} \"{assembly.FullName}\" " +
                $"({types.Length} {nameof(Type)}(s) in total)...");

            IEnumerable<Type> nonAbstractTypes = types
                .Where(x => !x.IsAbstract);

            this.loggerWrapper.Info(
                $"{nonAbstractTypes.Count()} {nameof(Type)}(s) enumerated. " +
                $"Constructing entities...");

            toReturn = nonAbstractTypes
                .Select(this.Map)
                .OrderBy(x => x.Name);

            return toReturn;
        }

        private Entity Map(Type type)
        {
            Entity toReturn = null;

            IEnumerable<PropertyInfo> propertyInfos = type.GetProperties();

            // We don't want collections. Filter these out.
            this.loggerWrapper.Debug(
                $"{propertyInfos.Count()} {nameof(PropertyInfo)}(s) " +
                $"enumerated for type {type.FullName}. Filtering out " +
                $"properties that are interfaces, as these are not " +
                $"applicable...");

            propertyInfos = propertyInfos
                 .Where(x => !x.PropertyType.IsInterface);

            this.loggerWrapper.Info(
                $"{propertyInfos.Count()} {nameof(PropertyInfo)}(s) " +
                $"filtered.");

            IEnumerable<Field> fields = propertyInfos
                .Select(this.Map)
                .OrderBy(x => x.Name);

            toReturn = new Entity()
            {
                Name = type.Name,
                Fields = fields,
                Sources = new string[]
                {
                    // Explicitly set this to null, so that it serialises
                    // out and can be fleshed out by whoever it is setting
                    // it up.
                    null,
                },
            };

            return toReturn;
        }

        private Field Map(PropertyInfo propertyInfo)
        {
            Field toReturn = new Field()
            {
                Name = propertyInfo.Name,

                // Treat false as a default value - doesn't have to be
                // explicitly specified, but may as well.
                TreatWhitespaceAsNull = false,
                Sources = new string[]
                {
                    // Explicitly set this to null, so that it serialises
                    // out and can be fleshed out by whoever it is setting
                    // it up.
                    null,
                },
            };

            return toReturn;
        }
    }
}