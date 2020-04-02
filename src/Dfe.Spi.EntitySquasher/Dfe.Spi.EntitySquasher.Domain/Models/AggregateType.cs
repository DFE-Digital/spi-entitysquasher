namespace Dfe.Spi.EntitySquasher.Domain.Models
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Describes the different types of aggregates, as supported by the
    /// squasher.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AggregateType
    {
        /// <summary>
        /// A "Count" type aggregate.
        /// </summary>
        Count,
    }
}