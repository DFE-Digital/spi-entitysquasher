namespace Dfe.Spi.EntitySquasher.Application
{
    using System;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions.SettingsProviders;
    using Dfe.Spi.EntitySquasher.Application.Models;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Models.Adcf;

    /// <summary>
    /// Implements <see cref="IGetSquashedEntityProcessor" />.
    /// </summary>
    public class GetSquashedEntityProcessor : IGetSquashedEntityProcessor
    {
        private readonly IAlgorithmDeclarationConfigurationFileManager algorithmDeclarationConfigurationFileManager;
        private readonly IGetSquashedEntityProcessorSettingsProvider getSquashedEntityProcessorSettingsProvider;
        private readonly ILoggerWrapper loggerWrapper;

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="GetSquashedEntityProcessor" /> class.
        /// </summary>
        /// <param name="algorithmDeclarationConfigurationFileManager">
        /// An instance of type
        /// <see cref="IAlgorithmDeclarationConfigurationFileManager" />.
        /// </param>
        /// <param name="getSquashedEntityProcessorSettingsProvider">
        /// An instance of type
        /// <see cref="IGetSquashedEntityProcessorSettingsProvider" />.
        /// </param>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        public GetSquashedEntityProcessor(
            IAlgorithmDeclarationConfigurationFileManager algorithmDeclarationConfigurationFileManager,
            IGetSquashedEntityProcessorSettingsProvider getSquashedEntityProcessorSettingsProvider,
            ILoggerWrapper loggerWrapper)
        {
            this.algorithmDeclarationConfigurationFileManager = algorithmDeclarationConfigurationFileManager;
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
                $"{nameof(AlgorithmDeclarationConfigurationFile)}...");

            AlgorithmDeclarationConfigurationFile algorithmDeclarationConfigurationFile =
                await this.algorithmDeclarationConfigurationFileManager.GetAlgorithmDeclarationConfigurationFileAsync(
                    algorithm)
                    .ConfigureAwait(false);

            this.loggerWrapper.Info(
                $"{nameof(algorithmDeclarationConfigurationFile)} = " +
                $"{algorithmDeclarationConfigurationFile}");

            // TODO:
            // 1) Pull from the requested adapters and;
            // 2) Squash the entities together according to the rules outlined
            //    in the ADCF.
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