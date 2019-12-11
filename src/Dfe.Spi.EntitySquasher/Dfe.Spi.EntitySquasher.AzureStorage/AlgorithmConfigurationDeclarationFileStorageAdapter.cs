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
    using Dfe.Spi.EntitySquasher.Domain.Models.Acdf;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Newtonsoft.Json;

    /// <summary>
    /// Implements
    /// <see cref="IAlgorithmConfigurationDeclarationFileStorageAdapter" />.
    /// </summary>
    public class AlgorithmConfigurationDeclarationFileStorageAdapter :
        IAlgorithmConfigurationDeclarationFileStorageAdapter
    {
        private readonly ILoggerWrapper loggerWrapper;

        private readonly CloudBlobClient cloudBlobClient;
        private readonly string acdfFilenameFormat;
        private readonly string acdfStorageContainerName;

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="AlgorithmConfigurationDeclarationFileStorageAdapter" />
        /// class.
        /// </summary>
        /// <param name="algorithmConfigurationDeclarationFileStorageAdapterSettingsProvider">
        /// An instance of type
        /// <see cref="IAlgorithmConfigurationDeclarationFileStorageAdapterSettingsProvider" />.
        /// </param>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        public AlgorithmConfigurationDeclarationFileStorageAdapter(
            IAlgorithmConfigurationDeclarationFileStorageAdapterSettingsProvider algorithmConfigurationDeclarationFileStorageAdapterSettingsProvider,
            ILoggerWrapper loggerWrapper)
        {
            if (algorithmConfigurationDeclarationFileStorageAdapterSettingsProvider == null)
            {
                throw new ArgumentNullException(
                    nameof(algorithmConfigurationDeclarationFileStorageAdapterSettingsProvider));
            }

            this.loggerWrapper = loggerWrapper;

            string acdfStorageConnectionString =
                algorithmConfigurationDeclarationFileStorageAdapterSettingsProvider.AcdfStorageConnectionString;

            CloudStorageAccount cloudStorageAccount =
                CloudStorageAccount.Parse(acdfStorageConnectionString);

            this.cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

            this.acdfFilenameFormat =
                algorithmConfigurationDeclarationFileStorageAdapterSettingsProvider.AcdfFilenameFormat;
            this.acdfStorageContainerName =
                algorithmConfigurationDeclarationFileStorageAdapterSettingsProvider.AcdfStorageContainerName;
        }

        /// <inheritdoc />
        public async Task<AlgorithmConfigurationDeclarationFile> GetAlgorithmConfigurationDeclarationFileAsync(
            string algorithm)
        {
            AlgorithmConfigurationDeclarationFile toReturn = null;

            string filename = string.Format(
                CultureInfo.InvariantCulture,
                this.acdfFilenameFormat,
                algorithm);

            IListBlobItem listBlobItem =
                await this.GetAlgorithmConfigurationDeclarationFileReferenceAsync(
                    filename)
                .ConfigureAwait(false);

            string container = this.acdfStorageContainerName;

            if (listBlobItem == null)
            {
                throw new FileNotFoundException(
                    $"Could not find the " +
                    $"{nameof(AlgorithmConfigurationDeclarationFile)} with " +
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

            toReturn = JsonConvert.DeserializeObject<AlgorithmConfigurationDeclarationFile>(
                fileContentRaw);

            this.loggerWrapper.Info($"File deserialised: {toReturn}.");

            return toReturn;
        }

        private static bool FilterForEventConfigurationDeclarationFile(
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

        private async Task<IListBlobItem> GetAlgorithmConfigurationDeclarationFileReferenceAsync(
            string filename)
        {
            IListBlobItem toReturn = null;

            CloudBlobContainer cloudBlobContainer =
                await this.GetContainerAsync().ConfigureAwait(false);

            string container = this.acdfStorageContainerName;

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
                x => FilterForEventConfigurationDeclarationFile(x, filename));

            return toReturn;
        }

        private async Task<CloudBlobContainer> GetContainerAsync()
        {
            CloudBlobContainer toReturn = null;

            string container = this.acdfStorageContainerName;

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