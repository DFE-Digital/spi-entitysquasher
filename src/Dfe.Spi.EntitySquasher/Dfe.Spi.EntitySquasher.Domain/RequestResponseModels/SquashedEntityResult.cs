namespace Dfe.Spi.EntitySquasher.Domain.RequestResponseModels
{
    public class SquashedEntityResult
    {
        public EntityReference EntityReference { get; set; }
        public object SquashedEntity { get; set; }
        public EntityAdapterErrorDetail[] EntityAdapterErrorDetails { get; set; }
    }
}