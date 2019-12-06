namespace Dfe.Spi.EntitySquasher.Domain.Definitions.SettingsProviders
{
    /// <summary>
    /// Describes the operations of the
    /// <see cref="IAlgorithmDeclarationConfigurationFileStorageAdapter" />
    /// settings provider.
    /// </summary>
    public interface IAlgorithmDeclarationConfigurationFileStorageAdapterSettingsProvider
    {
        /// <summary>
        /// Gets the filename format of the Algorithm Declaration Configuration
        /// Files.
        /// </summary>
        string AcdfFilenameFormat
        {
            get;
        }

        /// <summary>
        /// Gets the connection string to the storage account hosting the
        /// container which holds the Algorithm Declaration Configuration
        /// Files.
        /// </summary>
        string AdcfStorageConnectionString
        {
            get;
        }

        /// <summary>
        /// Gets the name of the container holding the Algorithm Declaration
        /// Configuration Files.
        /// </summary>
        string AdcfStorageContainerName
        {
            get;
        }
    }
}