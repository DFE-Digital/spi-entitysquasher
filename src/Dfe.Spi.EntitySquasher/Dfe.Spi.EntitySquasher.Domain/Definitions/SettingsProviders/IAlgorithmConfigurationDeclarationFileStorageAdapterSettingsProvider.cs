namespace Dfe.Spi.EntitySquasher.Domain.Definitions.SettingsProviders
{
    /// <summary>
    /// Describes the operations of the
    /// <see cref="IAlgorithmConfigurationDeclarationFileStorageAdapter" />
    /// settings provider.
    /// </summary>
    public interface IAlgorithmConfigurationDeclarationFileStorageAdapterSettingsProvider
    {
        /// <summary>
        /// Gets the filename format of the Algorithm Configuration Declaration
        /// Files.
        /// </summary>
        string AcdfFilenameFormat
        {
            get;
        }

        /// <summary>
        /// Gets the connection string to the storage account hosting the
        /// container which holds the Algorithm Configuration Declaration
        /// Files.
        /// </summary>
        string AcdfStorageConnectionString
        {
            get;
        }

        /// <summary>
        /// Gets the name of the container holding the Algorithm Configuration
        /// Declaration Files.
        /// </summary>
        string AcdfStorageContainerName
        {
            get;
        }
    }
}