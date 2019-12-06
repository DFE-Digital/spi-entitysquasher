namespace Dfe.Spi.EntitySquasher.FunctionApp.SettingsProviders
{
    using System;
    using Dfe.Spi.EntitySquasher.Domain.Definitions.SettingsProviders;

    /// <summary>
    /// Initialises a new instance of the
    /// <see cref="AlgorithmDeclarationConfigurationFileStorageAdapterSettingsProvider" />
    /// class.
    /// </summary>
    public class AlgorithmDeclarationConfigurationFileStorageAdapterSettingsProvider
        : IAlgorithmDeclarationConfigurationFileStorageAdapterSettingsProvider
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
        public string AdcfStorageConnectionString
        {
            get
            {
                string toReturn = Environment.GetEnvironmentVariable(
                    nameof(this.AdcfStorageConnectionString));

                return toReturn;
            }
        }

        /// <inheritdoc />
        public string AdcfStorageContainerName
        {
            get
            {
                string toReturn = Environment.GetEnvironmentVariable(
                    nameof(this.AdcfStorageContainerName));

                return toReturn;
            }
        }
    }
}