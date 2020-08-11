using System;
using Dfe.Spi.Common.Models;

namespace Dfe.Spi.EntitySquasher.Domain.RequestResponseModels
{
    public class SquashRequest : RequestResponseBase
    {
        public string EntityName { get; set; }
        public EntityReference[] EntityReferences { get; set; }
        public AggregatesRequest AggregatesRequest { get; set; }
        public string[] Fields { get; set; }
        public bool Live { get; set; }
        public DateTime? PointInTime { get; set; }
        public string Algorithm { get; set; }
    }
}