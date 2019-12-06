namespace Dfe.Spi.EntitySquasher.AzureStorage
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <summary>
    /// Contains static methods to assist in the listing of blobs.
    /// </summary>
    public static class ListBlobsHelper
    {
        /// <summary>
        /// Creates an effective <c>ListBlobs</c> method from the
        /// <paramref name="listBlobsSegmentedProviderAsync" /> method.
        /// </summary>
        /// <param name="listBlobsSegmentedProviderAsync">
        /// Provides the <c>ListBlobsSegmented</c> method.
        /// </param>
        /// <returns>
        /// An instance of <see cref="IEnumerable{IListBlobItem}" />.
        /// </returns>
        public static async Task<IEnumerable<IListBlobItem>> ListBlobsAsync(
            Func<BlobContinuationToken, Task<BlobResultSegment>> listBlobsSegmentedProviderAsync)
        {
            List<IListBlobItem> toReturn = new List<IListBlobItem>();

            // TODO: Common-ise with Transformation API.
            BlobContinuationToken blobContinuationToken = null;
            BlobResultSegment blobResultSegment = null;
            do
            {
                blobResultSegment = await listBlobsSegmentedProviderAsync(
                    blobContinuationToken)
                    .ConfigureAwait(false);

                blobContinuationToken = blobResultSegment.ContinuationToken;

                toReturn.AddRange(blobResultSegment.Results);
            }
            while (blobContinuationToken != null);

            return toReturn;
        }
    }
}