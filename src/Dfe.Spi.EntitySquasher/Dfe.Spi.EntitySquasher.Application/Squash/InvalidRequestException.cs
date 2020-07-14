using System;

namespace Dfe.Spi.EntitySquasher.Application.Squash
{
    public class InvalidRequestException : Exception
    {
        public InvalidRequestException(string message)
            : base(message)
        {
        }
    }
}