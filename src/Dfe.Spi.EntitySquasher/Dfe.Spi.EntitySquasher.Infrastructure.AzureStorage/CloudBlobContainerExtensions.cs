namespace Dfe.Spi.EntitySquasher.Infrastructure.AzureStorage
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <summary>
    /// Contains extension methods for the <see cref="CloudBlobContainer" />
    /// class.
    /// </summary>
    public static class CloudBlobContainerExtensions
    {
        /// <summary>
        /// Creates an effective <c>ListBlobs</c> method from the
        /// <see cref="CloudBlobContainer.ListBlobsSegmentedAsync(BlobContinuationToken)" />
        /// method.
        /// </summary>
        /// <param name="cloudBlobContainer">
        /// An instance of <see cref="CloudBlobContainer" />.
        /// </param>
        /// <returns>
        /// An instance of <see cref="IEnumerable{IListBlobItem}" />.
        /// </returns>
        public static async Task<IEnumerable<IListBlobItem>> ListBlobsAsync(
            this CloudBlobContainer cloudBlobContainer)
        {
            IEnumerable<IListBlobItem> toReturn = null;

            if (cloudBlobContainer == null)
            {
                throw new ArgumentNullException(nameof(cloudBlobContainer));
            }

            toReturn =
                await ListBlobsHelper.ListBlobsAsync(
                    cloudBlobContainer.ListBlobsSegmentedAsync)
                    .ConfigureAwait(false);

            return toReturn;
        }
    }
}