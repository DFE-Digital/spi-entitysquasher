using System;
using System.Net;
using Dfe.Spi.Common.Models;

namespace Dfe.Spi.EntitySquasher.Domain.Adapters
{
    public class DataAdapterException : Exception
    {
        public DataAdapterException(string message)
            : base(message)
        {
        }

        public DataAdapterException(string message, Exception innerException)
            : base(message)
        {
        }

        public string AdapterName { get; set; }
        public string RequestedEntityName { get; set; }
        public string[] RequestedIds { get; set; }
        public string[] RequestedFields { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }
        public HttpErrorBody HttpErrorBody { get; set; }
    }
}