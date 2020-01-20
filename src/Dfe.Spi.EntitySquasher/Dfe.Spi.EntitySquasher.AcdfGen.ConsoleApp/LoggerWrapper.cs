namespace Dfe.Spi.EntitySquasher.AcdfGen.ConsoleApp
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Dfe.Spi.Common.Logging.Definitions;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Implements <see cref="ILoggerWrapper" />.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class LoggerWrapper : ILoggerWrapper
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="LoggerWrapper" />
        /// class.
        /// </summary>
        public LoggerWrapper()
        {
            // Nothing for now.
        }

        /// <inheritdoc />
        public void Debug(string message, Exception exception = null)
        {
            this.WriteConsole(null, message, exception);
        }

        /// <inheritdoc />
        public void Error(string message, Exception exception = null)
        {
            this.WriteConsole(ConsoleColor.Red, message, exception);
        }

        /// <inheritdoc />
        public void Info(string message, Exception exception = null)
        {
            this.WriteConsole(ConsoleColor.Blue, message, exception);
        }

        /// <inheritdoc />
        public void SetContext(IHeaderDictionary headerDictionary)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void SetInternalRequestId(Guid internalRequestId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Warning(string message, Exception exception = null)
        {
            this.WriteConsole(ConsoleColor.Yellow, message, exception);
        }

        private void WriteConsole(
            ConsoleColor? consoleColor,
            string message,
            Exception exception = null)
        {
            if (consoleColor.HasValue)
            {
                Console.ForegroundColor = consoleColor.Value;
            }
            else
            {
                Console.ResetColor();
            }

            Console.WriteLine(message);

            if (exception != null)
            {
                Console.WriteLine(exception);
            }
        }
    }
}