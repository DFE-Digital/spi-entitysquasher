namespace Dfe.Spi.EntitySquasher.AzureStorage
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Definitions.SettingsProviders;
    using Dfe.Spi.EntitySquasher.Domain.Models.Adcf;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Newtonsoft.Json;

    /// <summary>
    /// Implements
    /// <see cref="IAlgorithmDeclarationConfigurationFileStorageAdapter" />.
    /// </summary>
    public class AlgorithmDeclarationConfigurationFileStorageAdapter :
        IAlgorithmDeclarationConfigurationFileStorageAdapter
    {
        private readonly ILoggerWrapper loggerWrapper;

        private readonly CloudBlobClient cloudBlobClient;
        private readonly string acdfFilenameFormat;
        private readonly string adcfStorageContainerName;

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="AlgorithmDeclarationConfigurationFileStorageAdapter" />
        /// class.
        /// </summary>
        /// <param name="algorithmDeclarationConfigurationFileStorageAdapterSettingsProvider">
        /// An instance of type
        /// <see cref="IAlgorithmDeclarationConfigurationFileStorageAdapterSettingsProvider" />.
        /// </param>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        public AlgorithmDeclarationConfigurationFileStorageAdapter(
            IAlgorithmDeclarationConfigurationFileStorageAdapterSettingsProvider algorithmDeclarationConfigurationFileStorageAdapterSettingsProvider,
            ILoggerWrapper loggerWrapper)
        {
            if (algorithmDeclarationConfigurationFileStorageAdapterSettingsProvider == null)
            {
                throw new ArgumentNullException(
                    nameof(algorithmDeclarationConfigurationFileStorageAdapterSettingsProvider));
            }

            this.loggerWrapper = loggerWrapper;

            string adcfStorageConnectionString =
                algorithmDeclarationConfigurationFileStorageAdapterSettingsProvider.AdcfStorageConnectionString;

            CloudStorageAccount cloudStorageAccount =
                CloudStorageAccount.Parse(adcfStorageConnectionString);

            this.cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

            this.acdfFilenameFormat =
                algorithmDeclarationConfigurationFileStorageAdapterSettingsProvider.AcdfFilenameFormat;
            this.adcfStorageContainerName =
                algorithmDeclarationConfigurationFileStorageAdapterSettingsProvider.AdcfStorageContainerName;
        }

        /// <inheritdoc />
        public async Task<AlgorithmDeclarationConfigurationFile> GetAlgorithmDeclarationConfigurationFileAsync(
            string algorithm)
        {
            AlgorithmDeclarationConfigurationFile toReturn = null;

            string filename = string.Format(
                CultureInfo.InvariantCulture,
                this.acdfFilenameFormat,
                algorithm);

            IListBlobItem listBlobItem =
                await this.GetAlgorithmDeclarationConfigurationFileReferenceAsync(
                    filename)
                .ConfigureAwait(false);

            string container = this.adcfStorageContainerName;

            if (listBlobItem == null)
            {
                throw new FileNotFoundException(
                    $"Could not find the " +
                    $"{nameof(AlgorithmDeclarationConfigurationFile)} with " +
                    $"filename \"{filename}\" in the root of container " +
                    $"\"{container}\" for the configured connection string!");
            }

            CloudBlockBlob cloudBlockBlob = listBlobItem as CloudBlockBlob;

            byte[] configFileBytes =
                new byte[cloudBlockBlob.Properties.Length];

            this.loggerWrapper.Debug(
                $"Found reference to \"{filename}\". Downloading " +
                $"{configFileBytes.Length} byte(s)...");

            await cloudBlockBlob.DownloadToByteArrayAsync(configFileBytes, 0)
                .ConfigureAwait(false);

            this.loggerWrapper.Info(
                $"File downloaded ({configFileBytes.Length} byte(s)). " +
                $"Deserialising into a managable format...");

            string fileContentRaw = null;
            using (MemoryStream memoryStream = new MemoryStream(configFileBytes))
            {
                using (StreamReader streamReader = new StreamReader(memoryStream))
                {
                    fileContentRaw = streamReader.ReadToEnd();
                }
            }

            toReturn = JsonConvert.DeserializeObject<AlgorithmDeclarationConfigurationFile>(
                fileContentRaw);

            this.loggerWrapper.Info($"File deserialised: {toReturn}.");

            return toReturn;
        }

        private bool FilterForEventConfigurationDeclarationFile(
            IListBlobItem listBlobItem,
            string filename)
        {
            bool toReturn = false;

            string uriStr = listBlobItem.Uri.AbsoluteUri;

            toReturn = uriStr.EndsWith(
                filename,
                StringComparison.InvariantCulture);

            return toReturn;
        }

        private async Task<IListBlobItem> GetAlgorithmDeclarationConfigurationFileReferenceAsync(
            string filename)
        {
            IListBlobItem toReturn = null;

            CloudBlobContainer cloudBlobContainer =
                await this.GetContainerAsync().ConfigureAwait(false);

            string container = this.adcfStorageContainerName;

            this.loggerWrapper.Debug(
                $"Listing all the files in the root of \"{container}\"...");

            IEnumerable<IListBlobItem> listBlobItems =
                await cloudBlobContainer.ListBlobsAsync()
                .ConfigureAwait(false);

            this.loggerWrapper.Info(
                $"Pulled back list of {listBlobItems.Count()} item(s) for " +
                $"container \"{container}\".");

            this.loggerWrapper.Debug($"Searching for \"{filename}\"...");

            toReturn = listBlobItems.SingleOrDefault(
                x => this.FilterForEventConfigurationDeclarationFile(x, filename));

            return toReturn;
        }

        private async Task<CloudBlobContainer> GetContainerAsync()
        {
            CloudBlobContainer toReturn = null;

            string container = this.adcfStorageContainerName;

            this.loggerWrapper.Debug(
                $"Getting container reference for \"{container}\"...");

            toReturn = this.cloudBlobClient.GetContainerReference(container);

            await toReturn.CreateIfNotExistsAsync().ConfigureAwait(false);

            this.loggerWrapper.Info(
                $"Container reference for \"{container}\" obtained.");

            return toReturn;
        }
    }
}