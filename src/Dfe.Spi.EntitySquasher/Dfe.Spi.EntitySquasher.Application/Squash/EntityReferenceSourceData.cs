using Dfe.Spi.EntitySquasher.Domain.RequestResponseModels;

namespace Dfe.Spi.EntitySquasher.Application.Squash
{
    public class EntityReferenceSourceData<T>
    {
        public EntityReference EntityReference { get; set; }
        public SourceSystemEntity<T>[] SourceEntities { get; set; }
    }
}