using System.Collections.Generic;
using System.Net;
using Dfe.Spi.Common.Models;

namespace Dfe.Spi.EntitySquasher.Domain.RequestResponseModels
{
    public class EntityAdapterErrorDetail
    {
        public string AdapterName { get; set; }
        public string RequestedEntityName { get; set; }
        public string RequestedId { get; set; }
        public IEnumerable<string> RequestedFields { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }
        public HttpErrorBody HttpErrorBody { get; set; }
    }
}