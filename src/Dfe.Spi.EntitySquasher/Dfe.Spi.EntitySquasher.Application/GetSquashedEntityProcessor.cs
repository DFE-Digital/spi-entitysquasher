namespace Dfe.Spi.EntitySquasher.Application
{
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Definitions;
    using Dfe.Spi.EntitySquasher.Application.Models;

    /// <summary>
    /// Implements <see cref="IGetSquashedEntityProcessor" />.
    /// </summary>
    public class GetSquashedEntityProcessor : IGetSquashedEntityProcessor
    {
        private readonly ILoggerWrapper loggerWrapper;

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="GetSquashedEntityProcessor" /> class.
        /// </summary>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        public GetSquashedEntityProcessor(ILoggerWrapper loggerWrapper)
        {
            this.loggerWrapper = loggerWrapper;
        }

        /// <inheritdoc />
        public GetSquashedEntityResponse GetSquashedEntity(
            GetSquashedEntityRequest getSquashedEntityRequest)
        {
            GetSquashedEntityResponse toReturn = null;

            this.loggerWrapper.Debug("Logging debug information...");

            // TODO...
            return toReturn;
        }
    }
}