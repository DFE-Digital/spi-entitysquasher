using System.Linq;

namespace Dfe.Spi.EntitySquasher.Domain.RequestResponseModels
{
    public class EntityReference
    {
        public AdapterRecordReference[] AdapterRecordReferences { get; set; }

        public override string ToString()
        {
            return AdapterRecordReferences
                .Select(x => $"[{x.SourceSystemName}:{x.SourceSystemId}]")
                .Aggregate((x, y) => $"{x}, {y}");
        }
    }
}