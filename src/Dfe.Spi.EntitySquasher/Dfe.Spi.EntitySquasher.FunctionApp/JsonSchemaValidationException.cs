namespace Dfe.Spi.EntitySquasher.FunctionApp
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using Newtonsoft.Json.Schema;
    using NJsonSchema.Validation;

    /// <summary>
    /// Represents an error raised upon finding <see cref="JsonSchema" />
    /// validation errors.
    /// </summary>
    [SuppressMessage(
        "Microsoft.Design",
        "CA1032",
        Justification = "This is internally used, and will not be serialised.")]
    public class JsonSchemaValidationException : Exception
    {
        private new const string Message =
            "The input JSON did not meet the requirements of the " +
            "schema.{0}{0}{1}";

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="JsonSchemaValidationException" /> class.
        /// </summary>
        /// <param name="validationErrors">
        /// A collection of <see cref="ValidationError" /> instances.
        /// </param>
        public JsonSchemaValidationException(
            ICollection<ValidationError> validationErrors)
            : base(BuildExceptionMessage(validationErrors))
        {
            this.ValidationErrors = validationErrors;
        }

        /// <summary>
        /// Gets a collection of <see cref="ValidationError" /> instances.
        /// </summary>
        public ICollection<ValidationError> ValidationErrors
        {
            get;
            private set;
        }

        private static string BuildExceptionMessage(
            ICollection<ValidationError> validationErrors)
        {
            string toReturn = null;

            string[] errorListArr = validationErrors
                .Select(x => $"* {x}")
                .ToArray();

            string newLine = Environment.NewLine;

            string errorList = string.Join(newLine, errorListArr);

            toReturn = string.Format(
                CultureInfo.InvariantCulture,
                Message,
                newLine,
                errorList);

            return toReturn;
        }
    }
}