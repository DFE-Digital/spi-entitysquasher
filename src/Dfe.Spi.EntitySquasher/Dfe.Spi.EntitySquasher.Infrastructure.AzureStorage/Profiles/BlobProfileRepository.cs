using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Dfe.Spi.EntitySquasher.Domain.Configuration;
using Dfe.Spi.EntitySquasher.Domain.Profiles;
using Newtonsoft.Json;

namespace Dfe.Spi.EntitySquasher.Infrastructure.AzureStorage.Profiles
{
    public class BlobProfileRepository : IProfileRepository
    {
        private BlobContainerClient _containerClient;

        public BlobProfileRepository(EntitySquasherConfiguration configuration)
        {
            var serviceClient = new BlobServiceClient(configuration.Profile.BlobConnectionString);
            _containerClient = serviceClient.GetBlobContainerClient(configuration.Profile.ContainerName);
        }

        public async Task<Profile> GetProfileAsync(string name, CancellationToken cancellationToken)
        {
            var blob = _containerClient.GetBlobClient($"acdf-{name}.json");
            var data = await blob.DownloadAsync(cancellationToken);
            
            using var reader = new StreamReader(data.Value.Content);
            var json = await reader.ReadToEndAsync();

            var profile = JsonConvert.DeserializeObject<Profile>(json);
            profile.Name = name;
            
            return profile;
        }
    }
}