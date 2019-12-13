namespace Dfe.Spi.EntitySquasher.Application.Models
{
    using Dfe.Spi.EntitySquasher.Application.Definitions.Caches;

    /// <summary>
    /// Key type, used by
    /// <see cref="IEntityAdapterClientCache" />.
    /// </summary>
    public class EntityAdapterClientKey : ModelsBase
    {
        /// <summary>
        /// Gets or sets the algorithm containing the entity
        /// adapter configuration.
        /// </summary>
        public string Algorithm
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the entity adapter.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            bool toReturn = false;

            EntityAdapterClientKey entityAdapterClientKey =
                obj as EntityAdapterClientKey;

            if (entityAdapterClientKey != null)
            {
                string algorithm =
                    entityAdapterClientKey.Algorithm;
                string name = entityAdapterClientKey.Name;

                toReturn =
                    (this.Algorithm == algorithm)
                        &&
                    (this.Name == name);
            }

            return toReturn;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            // Method of coming up with a good hashcode
            // taken from the following article:
            // https://www.loganfranken.com/blog/692/overriding-equals-in-c-part-2/
            unchecked
            {
                int toReturn = 13;

                toReturn = (toReturn * 7) + (!object.ReferenceEquals(null, this.Algorithm) ? this.Algorithm.GetHashCode() : 0);
                toReturn = (toReturn * 7) + (!object.ReferenceEquals(null, this.Name) ? this.Name.GetHashCode() : 0);

                return toReturn;
            }
        }
    }
}