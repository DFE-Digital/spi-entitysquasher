using System;
using Dfe.Spi.Common.Logging.Definitions;

namespace Dfe.Spi.EntitySquasher.ProfileGenerator
{
    public class Logger : ILoggerWrapper
    {
        public void Debug(string message, Exception exception = null)
        {
            var logMessage = exception == null
                ? message
                : $"{message}\n{exception}";
            WriteColored(logMessage, Console.ForegroundColor);
        }

        public void Info(string message, Exception exception = null)
        {
            var logMessage = exception == null
                ? message
                : $"{message}\n{exception}";
            WriteColored(logMessage, ConsoleColor.Cyan);
        }

        public void Warning(string message, Exception exception = null)
        {
            var logMessage = exception == null
                ? message
                : $"{message}\n{exception}";
            WriteColored(logMessage, ConsoleColor.Yellow);
        }

        public void Error(string message, Exception exception = null)
        {
            var logMessage = exception == null
                ? message
                : $"{message}\n{exception}";
            WriteColored(logMessage, ConsoleColor.Red);
        }
        
        private void WriteColored(string message, ConsoleColor color)
        {
            try
            {
                Console.ForegroundColor = color;
                Console.WriteLine(message);
            }
            finally
            {
                Console.ResetColor();
            }
        }
    }
}