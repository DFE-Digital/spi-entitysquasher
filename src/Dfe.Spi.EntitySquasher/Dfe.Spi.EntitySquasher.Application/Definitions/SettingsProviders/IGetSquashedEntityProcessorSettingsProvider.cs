namespace Dfe.Spi.EntitySquasher.Application.Definitions.SettingsProviders
{
    using Dfe.Spi.EntitySquasher.Application.Processors.Definitions;

    /// <summary>
    /// Describes the operations of the
    /// <see cref="IGetSquashedEntityProcessor" /> settings provider.
    /// </summary>
    public interface IGetSquashedEntityProcessorSettingsProvider
    {
        /// <summary>
        /// Gets the default algorithm.
        /// </summary>
        string DefaultAlgorithm
        {
            get;
        }
    }
}