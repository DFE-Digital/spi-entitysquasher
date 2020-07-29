namespace Dfe.Spi.EntitySquasher.Domain.Adapters
{
    public class DataAdapterResult<T>
    {
        public string Identifier { get; set; }
        public T Entity { get; set; }
        public DataAdapterException AdapterError { get; set; }
    }
}