using Dfe.Spi.EntitySquasher.Domain.Adapters;

namespace Dfe.Spi.EntitySquasher.Application.Squash
{
    public class SourceSystemEntity<T>
    {
        public string SourceName { get; set; }
        public string SourceId { get; set; }
        public T Entity { get; set; }
        public DataAdapterException AdapterError { get; set; }
    }
}