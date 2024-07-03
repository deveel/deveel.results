using System.ComponentModel.DataAnnotations;

namespace Deveel
{
    /// <summary>
    /// Represents an error that occurred while validating a
    /// a command or operation.
    /// </summary>
    public interface IValidationError : IOperationError
    {
        /// <summary>
        /// Gets the list of validation results that caused the error.
        /// </summary>
        IReadOnlyList<ValidationResult> ValidationResults { get; }
    }
}
