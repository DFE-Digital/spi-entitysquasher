namespace Dfe.Spi.EntitySquasher.Application.Models
{
    /// <summary>
    /// Represents the result of a squash operation for a given
    /// <see cref="Models.EntityReference" /> request.
    /// </summary>
    public class SquashedEntityResult : EntityAdapterResult
    {
        /// <summary>
        /// Gets or sets the originally requested
        /// <see cref="Models.EntityReference" />.
        /// </summary>
        public EntityReference EntityReference
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the requested squashed entity.
        /// </summary>
        public Spi.Models.ModelsBase SquashedEntity
        {
            get;
            set;
        }
    }
}