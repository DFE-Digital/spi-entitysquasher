namespace Dfe.Spi.EntitySquasher.AcdfGen.Infrastructure.IO
{
    using System.IO;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.AcdfGen.Domain.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Models.Acdf;
    using Newtonsoft.Json;

    /// <summary>
    /// Implements
    /// <see cref="IGeneratedAlgorithmConfigurationDeclarationFileRepository" />.
    /// </summary>
    public class GeneratedAlgorithmConfigurationDeclarationFileRepository
        : IGeneratedAlgorithmConfigurationDeclarationFileRepository
    {
        private readonly ILoggerWrapper loggerWrapper;

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="GeneratedAlgorithmConfigurationDeclarationFileRepository" />
        /// class.
        /// </summary>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        public GeneratedAlgorithmConfigurationDeclarationFileRepository(
            ILoggerWrapper loggerWrapper)
        {
            this.loggerWrapper = loggerWrapper;
        }

        /// <inheritdoc />
        public string Save(
            string filename,
            AlgorithmConfigurationDeclarationFile algorithmConfigurationDeclarationFile)
        {
            string toReturn = null;

            string algorithmConfigurationDeclarationFileStr =
                JsonConvert.SerializeObject(
                    algorithmConfigurationDeclarationFile,
                    Formatting.Indented);

            this.loggerWrapper.Debug(
                $"{nameof(algorithmConfigurationDeclarationFileStr)} = " +
                $"\"{algorithmConfigurationDeclarationFile}\"");

            FileInfo fileInfo = new FileInfo(filename);

            toReturn = fileInfo.FullName;

            this.loggerWrapper.Info(
                $"Destination location for " +
                $"{nameof(AlgorithmConfigurationDeclarationFile)}: " +
                $"\"{toReturn}\".");

            using (FileStream fileStream = fileInfo.Open(FileMode.Create))
            {
                using (StreamWriter streamWriter = new StreamWriter(fileStream))
                {
                    streamWriter.Write(
                        algorithmConfigurationDeclarationFileStr);
                }
            }

            this.loggerWrapper.Info($"{fileInfo.FullName} saved.");

            return toReturn;
        }
    }
}