namespace Dfe.Spi.EntitySquasher.Application.Definitions.Factories
{
    using Dfe.Spi.Common.Caching.Definitions.Factories.Managers;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Managers;
    using Dfe.Spi.EntitySquasher.Domain.Models.Acdf;

    /// <summary>
    /// Describes the operations of the
    /// <see cref="IAlgorithmConfigurationDeclarationFileManager" /> factory.
    /// </summary>
    public interface IAlgorithmConfigurationDeclarationFileManagerFactory
        : ICacheManagerFactory<string, AlgorithmConfigurationDeclarationFile>
    {
        /// <summary>
        /// Creates an instance of the
        /// <see cref="IAlgorithmConfigurationDeclarationFileManager" /> class.
        /// </summary>
        /// <returns>
        /// An instance of type
        /// <see cref="IAlgorithmConfigurationDeclarationFileManager" />.
        /// </returns>
        IAlgorithmConfigurationDeclarationFileManager Create();
    }
}