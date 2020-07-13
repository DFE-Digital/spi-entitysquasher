using System.Threading;
using System.Threading.Tasks;

namespace Dfe.Spi.EntitySquasher.Domain.Profiles
{
    public interface IProfileRepository
    {
        Task<Profile> GetProfileAsync(string name, CancellationToken cancellationToken);
    }
}