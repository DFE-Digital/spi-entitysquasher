namespace Dfe.Spi.EntitySquasher.Application.Factories
{
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Factories;

    /// <summary>
    /// Implements <see cref="IGetSquashedEntityProcessor" />.
    /// </summary>
    public class GetSquashedEntityProcessorFactory
        : IGetSquashedEntityProcessorFactory
    {
        /// <inheritdoc />
        public IGetSquashedEntityProcessor Create(ILoggerWrapper loggerWrapper)
        {
            GetSquashedEntityProcessor toReturn =
                new GetSquashedEntityProcessor(
                    loggerWrapper);

            return toReturn;
        }
    }
}