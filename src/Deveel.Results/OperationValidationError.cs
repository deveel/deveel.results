using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Deveel
{
    /// <summary>
    /// An error that occurred during the validation of an operation.
    /// </summary>
    public readonly struct OperationValidationError : IValidationError
    {
        /// <summary>
        /// Constructs an instance of an <see cref="OperationValidationError"/> object.
        /// </summary>
        /// <param name="code">
        /// The code of the error, that is unique within the
        /// given domain.
        /// </param>
        /// <param name="domain">
        /// The domain where the error occurred.
        /// </param>
        /// <param name="validationResults">
        /// A list of validation results that caused the error.
        /// </param>
        public OperationValidationError(string code, string domain, IReadOnlyList<ValidationResult> validationResults)
        {
            ArgumentNullException.ThrowIfNull(code, nameof(code));
            ArgumentNullException.ThrowIfNull(domain, nameof(domain));
            ArgumentNullException.ThrowIfNull(validationResults, nameof(validationResults));

            Code = code;
            Domain = domain;
            ValidationResults = validationResults;
        }

        /// <inheritdoc/>
        public IReadOnlyList<ValidationResult> ValidationResults { get; }

        /// <inheritdoc/>
        public string Code { get; }

        /// <inheritdoc/>
        public string Domain { get; }

        [ExcludeFromCodeCoverage]
        string? IOperationError.Message => null;

        [ExcludeFromCodeCoverage]
        IOperationError? IOperationError.InnerError => null;
    }
}
