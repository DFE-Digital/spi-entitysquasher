namespace Dfe.Spi.EntitySquasher.Application.Processors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions.SettingsProviders;
    using Dfe.Spi.EntitySquasher.Application.Models;
    using Dfe.Spi.EntitySquasher.Application.Processors.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Models.Acdf;

    /// <summary>
    /// Implements <see cref="IGetSquashedEntityProcessor" />.
    /// </summary>
    public class GetSquashedEntityProcessor : IGetSquashedEntityProcessor
    {
        private readonly IAlgorithmConfigurationDeclarationFileManager algorithmConfigurationDeclarationFileManager;
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
        /// <param name="getSquashedEntityProcessorSettingsProvider">
        /// An instance of type
        /// <see cref="IGetSquashedEntityProcessorSettingsProvider" />.
        /// </param>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        public GetSquashedEntityProcessor(
            IAlgorithmConfigurationDeclarationFileManager algorithmConfigurationDeclarationFileManager,
            IGetSquashedEntityProcessorSettingsProvider getSquashedEntityProcessorSettingsProvider,
            ILoggerWrapper loggerWrapper)
        {
            this.algorithmConfigurationDeclarationFileManager = algorithmConfigurationDeclarationFileManager;
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

            AlgorithmConfigurationDeclarationFile algorithmConfigurationDeclarationFile =
                await this.algorithmConfigurationDeclarationFileManager.GetAlgorithmConfigurationDeclarationFileAsync(
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

            IEnumerable<EntityReference> entityReferences =
                getSquashedEntityRequest.EntityReferences;

            IEnumerable<Spi.Models.ModelsBase> entities = entityReferences
                .Select(x => this.ProcessSingleEntityReference(entityName, x));

            toReturn = new GetSquashedEntityResponse()
            {
                Entities = entities,
            };

            return toReturn;
        }

        private Spi.Models.ModelsBase ProcessSingleEntityReference(
            string entityName,
            EntityReference entityReference)
        {
            Spi.Models.ModelsBase toReturn = null;

            // TODO:
            // 1) Call all adapters specified in the entity reference
            //    at the same time. Pull back the requested ModelBase.
            // 2) Perform the squashing.
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