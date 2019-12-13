namespace Dfe.Spi.EntitySquasher.Application.Definitions.Managers
{
    using System.Threading.Tasks;

    /// <summary>
    /// Describes the operations of a manager.
    /// </summary>
    /// <typeparam name="TManagerItem">
    /// The type of item being managed.
    /// </typeparam>
    public interface IManager<TManagerItem>
        where TManagerItem : class
    {
        /// <summary>
        /// Gets an item from the manager.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// An instance of type <typeparamref name="TManagerItem" />.
        /// </returns>
        Task<TManagerItem> GetAsync(string key);
    }
}