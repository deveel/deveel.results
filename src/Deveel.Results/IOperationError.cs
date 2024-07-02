namespace Deveel
{
    /// <summary>
    /// Represents an error that occurred during an operation.
    /// </summary>
    public interface IOperationError
    {
        /// <summary>
        /// Gets the unique code of the error
        /// within the domain of the operation.
        /// </summary>
        string Code { get; }

        /// <summary>
        /// Gets the domain of the operation
        /// where the error occurred.
        /// </summary>
        string Domain { get; }

        /// <summary>
        /// Gets the message that describes the error.
        /// </summary>
        string? Message { get; }

        /// <summary>
        /// Gets an inner error that caused this error.
        /// </summary>
        IOperationError? InnerError { get; }
    }
}
