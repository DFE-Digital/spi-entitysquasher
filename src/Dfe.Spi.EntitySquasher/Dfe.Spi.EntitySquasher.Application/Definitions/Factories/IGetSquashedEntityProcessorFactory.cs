namespace Dfe.Spi.EntitySquasher.Application.Definitions.Factories
{
    using Dfe.Spi.Common.Logging.Definitions;

    /// <summary>
    /// Describes the operations of the
    /// <see cref="IGetSquashedEntityProcessor" /> factory.
    /// </summary>
    public interface IGetSquashedEntityProcessorFactory
    {
        /// <summary>
        /// Creates an instance of type
        /// <see cref="IGetSquashedEntityProcessor" />.
        /// </summary>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        /// <returns>
        /// An instance of type <see cref="IGetSquashedEntityProcessor" />.
        /// </returns>
        IGetSquashedEntityProcessor Create(ILoggerWrapper loggerWrapper);
    }
}