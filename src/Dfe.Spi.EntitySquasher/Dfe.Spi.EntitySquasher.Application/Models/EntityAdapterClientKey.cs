namespace Dfe.Spi.EntitySquasher.Application.Models
{
    using System;
    using System.Linq;
    using System.Text;
    using Dfe.Spi.EntitySquasher.Application.Definitions.Caches;

    /// <summary>
    /// Key type, used by
    /// <see cref="IEntityAdapterClientCache" />.
    /// </summary>
    public class EntityAdapterClientKey : ModelsBase
    {
        private const char StringRepresentationDelimiter = '.';

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

        /// <summary>
        /// Parses an instance of <see cref="EntityAdapterClientKey" /> from
        /// its <see cref="string" /> representation.
        /// </summary>
        /// <param name="value">
        /// The <see cref="string" /> representation of an
        /// <see cref="EntityAdapterClientKey" />.
        /// </param>
        /// <returns>
        /// An instance of <see cref="EntityAdapterClientKey" />.
        /// </returns>
        public static EntityAdapterClientKey Parse(string value)
        {
            EntityAdapterClientKey toReturn = null;

            if (!string.IsNullOrEmpty(value))
            {
                string[] parts = value.Split(
                    new char[] { StringRepresentationDelimiter },
                    StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 2)
                {
                    string algorithm = parts.First();
                    algorithm = Base64Decode(algorithm);

                    string name = parts.Last();
                    name = Base64Decode(name);

                    toReturn = new EntityAdapterClientKey()
                    {
                        Algorithm = algorithm,
                        Name = name,
                    };
                }
            }

            if (toReturn == null)
            {
                throw new FormatException(
                    $"Could not parse the string \"{value}\" into an " +
                    $"instance of {nameof(EntityAdapterClientKey)}.");
            }

            return toReturn;
        }

        /// <summary>
        /// Exports this instance to a <see cref="string" /> value. This
        /// <see cref="string" /> value can then be used to create an instance
        /// of <see cref="EntityAdapterClientKey" /> via
        /// <see cref="Parse(string)" />.
        /// </summary>
        /// <returns>
        /// The exported <see cref="string" /> representation of this instance.
        /// </returns>
        public string ExportToString()
        {
            string toReturn = null;

            string encodedAlgorithm = Base64Encode(this.Algorithm);
            string encodedName = Base64Encode(this.Name);

            toReturn =
                encodedAlgorithm + StringRepresentationDelimiter + encodedName;

            return toReturn;
        }

        private static string Base64Decode(string base64EncodedData)
        {
            string toReturn = null;

            byte[] base64EncodedBytes = Convert.FromBase64String(
                base64EncodedData);

            toReturn = Encoding.UTF8.GetString(base64EncodedBytes);

            return toReturn;
        }

        private static string Base64Encode(string plainText)
        {
            string toReturn = null;

            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            toReturn = Convert.ToBase64String(plainTextBytes);

            return toReturn;
        }
    }
}