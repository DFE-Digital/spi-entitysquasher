namespace Dfe.Spi.EntitySquasher.AzureStorage
{
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Models.Adcf;

    /// <summary>
    /// Implements
    /// <see cref="IAlgorithmDeclarationConfigurationFileManager" />.
    /// </summary>
    public class AlgorithmDeclarationConfigurationFileManager
        : IAlgorithmDeclarationConfigurationFileManager
    {
        private readonly ILoggerWrapper loggerWrapper;

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="AlgorithmDeclarationConfigurationFileManager" /> class.
        /// </summary>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        public AlgorithmDeclarationConfigurationFileManager(
            ILoggerWrapper loggerWrapper)
        {
            this.loggerWrapper = loggerWrapper;
        }

        /// <inheritdoc />
        public AlgorithmDeclarationConfigurationFile GetAlgorithmDeclarationConfigurationFile(
            string algoritm)
        {
            this.loggerWrapper.Debug("This is a log message. TODO.");

            throw new System.NotImplementedException();
        }
    }
}