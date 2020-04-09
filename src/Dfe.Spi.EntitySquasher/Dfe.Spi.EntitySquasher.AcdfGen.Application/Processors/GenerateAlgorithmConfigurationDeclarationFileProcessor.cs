namespace Dfe.Spi.EntitySquasher.AcdfGen.Application.Processors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.AcdfGen.Application.Definitions.Processors;
    using Dfe.Spi.EntitySquasher.AcdfGen.Application.Models;
    using Dfe.Spi.EntitySquasher.AcdfGen.Domain.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Models.Acdf;
    using Dfe.Spi.Models.Entities;

    /// <summary>
    /// Implements
    /// <see cref="IGenerateAlgorithmConfigurationDeclarationFileProcessor" />.
    /// </summary>
    public class GenerateAlgorithmConfigurationDeclarationFileProcessor :
        IGenerateAlgorithmConfigurationDeclarationFileProcessor
    {
        private readonly IGeneratedAlgorithmConfigurationDeclarationFileRepository generatedAlgorithmConfigurationDeclarationFileRepository;
        private readonly ILoggerWrapper loggerWrapper;

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="GenerateAlgorithmConfigurationDeclarationFileProcessor" />
        /// class.
        /// </summary>
        /// <param name="generatedAlgorithmConfigurationDeclarationFileRepository">
        /// An instance of type
        /// <see cref="IGeneratedAlgorithmConfigurationDeclarationFileRepository" />.
        /// </param>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        public GenerateAlgorithmConfigurationDeclarationFileProcessor(
            IGeneratedAlgorithmConfigurationDeclarationFileRepository generatedAlgorithmConfigurationDeclarationFileRepository,
            ILoggerWrapper loggerWrapper)
        {
            this.generatedAlgorithmConfigurationDeclarationFileRepository = generatedAlgorithmConfigurationDeclarationFileRepository;
            this.loggerWrapper = loggerWrapper;
        }

        /// <inheritdoc />
        public GenerateAlgorithmConfigurationDeclarationFileResponse GenerateAlgorithmConfigurationDeclarationFile(
            GenerateAlgorithmConfigurationDeclarationFileRequest generateAlgorithmConfigurationDeclarationFileRequest)
        {
            GenerateAlgorithmConfigurationDeclarationFileResponse toReturn = null;

            if (generateAlgorithmConfigurationDeclarationFileRequest == null)
            {
                throw new ArgumentNullException(
                    nameof(generateAlgorithmConfigurationDeclarationFileRequest));
            }

            // 1) Use reflection to cycle through the concrete types available
            //    in Dfe.Spi.Models. Generate the majority of the file.
            this.loggerWrapper.Debug($"Generating {nameof(Entity)}(s)...");

            IEnumerable<Entity> entities = this.GenerateEntities();

            this.loggerWrapper.Info(
                $"{entities.Count()} {nameof(Entity)}(s) generated.");

            // 2) Generate placeholders for the adapters based on the input.
            IEnumerable<string> adapterNames =
                generateAlgorithmConfigurationDeclarationFileRequest.AdapterNames;

            this.loggerWrapper.Debug(
                $"Generating {nameof(EntityAdapter)}(s)...");

            IEnumerable<EntityAdapter> entityAdapters =
                this.GenerateEntityAdapters(adapterNames);

            this.loggerWrapper.Info(
                $"{entityAdapters.Count()} {nameof(EntityAdapter)}(s) " +
                $"generated.");

            // Construct the instance fully.
            AlgorithmConfigurationDeclarationFile algorithmConfigurationDeclarationFile =
                new AlgorithmConfigurationDeclarationFile()
                {
                    Entities = entities,
                    EntityAdapters = entityAdapters,
                };

            this.loggerWrapper.Debug(
                $"{nameof(algorithmConfigurationDeclarationFile)} = " +
                $"{algorithmConfigurationDeclarationFile}");

            // 3) Save the file with a filename based on input.
            this.loggerWrapper.Debug(
                $"Saving {nameof(algorithmConfigurationDeclarationFile)} to " +
                $"underlying storage...");

            string filename =
                generateAlgorithmConfigurationDeclarationFileRequest.Filename;

            string location =
                this.generatedAlgorithmConfigurationDeclarationFileRepository.Save(
                    filename,
                    algorithmConfigurationDeclarationFile);

            this.loggerWrapper.Info($"File saved to storage: \"{location}\".");

            toReturn = new GenerateAlgorithmConfigurationDeclarationFileResponse()
            {
                // Nothing for now.
                // Just return an instance to indicate success.
            };

            return toReturn;
        }

        private IEnumerable<EntityAdapter> GenerateEntityAdapters(
            IEnumerable<string> adapterNames)
        {
            IEnumerable<EntityAdapter> toReturn = null;

            toReturn = adapterNames.Select(this.Map).ToArray();

            return toReturn;
        }

        private IEnumerable<Entity> GenerateEntities()
        {
            IEnumerable<Entity> toReturn = null;

            // Use ModelsBase as a means to getting a reference to the
            // assembly.
            Type modelsBaseType = typeof(EntityBase);

            Assembly assembly = modelsBaseType.Assembly;

            Type[] types = assembly.GetTypes();

            this.loggerWrapper.Debug(
                $"Enumerating non-abstract types from the " +
                $"{nameof(Assembly)} \"{assembly.FullName}\" " +
                $"({types.Length} {nameof(Type)}(s) in total) that inherit " +
                $"from {nameof(EntityBase)}...");

            IEnumerable<Type> nonAbstractTypes = types
                .Where(x => !x.IsAbstract && x.BaseType == typeof(EntityBase));

            this.loggerWrapper.Info(
                $"{nonAbstractTypes.Count()} {nameof(Type)}(s) enumerated. " +
                $"Constructing entities...");

            toReturn = nonAbstractTypes
                .Select(this.Map)
                .OrderBy(x => x.Name)
                .ToArray();

            return toReturn;
        }

        private EntityAdapter Map(string name)
        {
            EntityAdapter toReturn = new EntityAdapter()
            {
                BaseUrl = null,
                Headers = null,
                Name = name,
            };

            this.loggerWrapper.Info(
                $"Generated {nameof(EntityAdapter)} with " +
                $"{nameof(toReturn.Name)} = \"{toReturn.Name}\".");

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
                .OrderBy(x => x.Name)
                .ToArray();

            toReturn = new Entity()
            {
                Name = type.Name,
                Fields = fields,
                Sources = null,
            };

            this.loggerWrapper.Info(
                $"Generated {nameof(Entity)} with {nameof(toReturn.Name)} = " +
                $"\"{toReturn.Name}\".");

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
                Sources = null,
            };

            this.loggerWrapper.Info(
                $"Generated {nameof(Field)} with {nameof(toReturn.Name)} = " +
                $"\"{toReturn.Name}\".");

            return toReturn;
        }
    }
}