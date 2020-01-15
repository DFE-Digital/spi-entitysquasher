namespace Dfe.Spi.EntitySquasher.AcdfGen.Application.Processors
{
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.AcdfGen.Application.Definitions.Processors;
    using Dfe.Spi.EntitySquasher.AcdfGen.Application.Models;

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

            this.loggerWrapper.Debug("This is a debug message.");
            this.loggerWrapper.Info("This is an info message.");
            this.loggerWrapper.Warning("This is a warning.");
            this.loggerWrapper.Error(
                "This is an error.",
                new System.Exception("An example exception."));

            return toReturn;
        }
    }
}