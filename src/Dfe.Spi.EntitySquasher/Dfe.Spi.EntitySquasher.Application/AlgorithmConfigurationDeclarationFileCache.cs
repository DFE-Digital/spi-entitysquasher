namespace Dfe.Spi.EntitySquasher.Application
{
    using System.Collections.Generic;
    using Dfe.Spi.EntitySquasher.Application.Definitions;
    using Dfe.Spi.EntitySquasher.Domain.Models.Acdf;

    /// <summary>
    /// Implements <see cref="IAlgorithmConfigurationDeclarationFileCache" />.
    /// </summary>
    public class AlgorithmConfigurationDeclarationFileCache
        : IAlgorithmConfigurationDeclarationFileCache
    {
        private readonly Dictionary<string, AlgorithmConfigurationDeclarationFile> cache;

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="AlgorithmConfigurationDeclarationFileCache" /> class.
        /// </summary>
        public AlgorithmConfigurationDeclarationFileCache()
        {
            this.cache =
                new Dictionary<string, AlgorithmConfigurationDeclarationFile>();
        }

        /// <inheritdoc />
        public void AddAlgorithmConfigurationDeclarationFile(
            string algorithm,
            AlgorithmConfigurationDeclarationFile algorithmConfigurationDeclarationFile)
        {
            // We should never need to overwrite what's in the cache.
            if (!this.cache.ContainsKey(algorithm))
            {
                // ... so just check that the key doesn't exist.
                this.cache.Add(algorithm, algorithmConfigurationDeclarationFile);
            }
        }

        /// <inheritdoc />
        public AlgorithmConfigurationDeclarationFile GetAlgorithmConfigurationDeclarationFile(
            string algorithm)
        {
            AlgorithmConfigurationDeclarationFile toReturn = null;

            if (this.cache.ContainsKey(algorithm))
            {
                toReturn = this.cache[algorithm];
            }

            return toReturn;
        }
    }
}