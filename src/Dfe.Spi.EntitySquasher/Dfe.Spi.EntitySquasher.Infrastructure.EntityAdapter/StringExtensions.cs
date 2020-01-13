namespace Dfe.Spi.EntitySquasher.Infrastructure.EntityAdapter
{
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Contains extension methods for the <see cref="string" /> class.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Converts a string from PascalCase to kebab-case.
        /// Method based on answer from
        /// <see href="https://stackoverflow.com/questions/37301287/how-do-i-convert-pascalcase-to-kebab-case-with-c" />.
        /// </summary>
        /// <param name="value">
        /// The string in PascalFormat, to convert to kebab-case.
        /// </param>
        /// <returns>
        /// The original <paramref name="value" />, but in kebab-case.
        /// </returns>
        [SuppressMessage(
            "Microsoft.Globalization",
            "CA1308",
            Justification = "I need lowercase for kebab-case, not uppercase.")]
        public static string PascalToKebabCase(this string value)
        {
            string toReturn = Regex.Replace(
                value,
                "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])",
                "-$1",
                RegexOptions.Compiled)
                .Trim()
                .ToLower(CultureInfo.InvariantCulture);

            return toReturn;
        }
    }
}