namespace Dfe.Spi.EntitySquasher.FunctionApp.SettingsProviders
{
    using System;
    using Dfe.Spi.EntitySquasher.Domain.Definitions.SettingsProviders;

    /// <summary>
    /// Initialises a new instance of the
    /// <see cref="AlgorithmConfigurationDeclarationFileStorageAdapterSettingsProvider" />
    /// class.
    /// </summary>
    public class AlgorithmConfigurationDeclarationFileStorageAdapterSettingsProvider
        : IAlgorithmConfigurationDeclarationFileStorageAdapterSettingsProvider
    {
        /// <inheritdoc />
        public string AcdfFilenameFormat
        {
            get
            {
                string toReturn = Environment.GetEnvironmentVariable(
                    nameof(this.AcdfFilenameFormat));

                return toReturn;
            }
        }

        /// <inheritdoc />
        public string AcdfStorageConnectionString
        {
            get
            {
                string toReturn = Environment.GetEnvironmentVariable(
                    nameof(this.AcdfStorageConnectionString));

                return toReturn;
            }
        }

        /// <inheritdoc />
        public string AcdfStorageContainerName
        {
            get
            {
                string toReturn = Environment.GetEnvironmentVariable(
                    nameof(this.AcdfStorageContainerName));

                return toReturn;
            }
        }
    }
}