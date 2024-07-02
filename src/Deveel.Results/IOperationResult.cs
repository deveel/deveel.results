namespace Deveel
{
    /// <summary>
    /// Represents the result of an operation.
    /// </summary>
    public interface IOperationResult
    {
        /// <summary>
        /// Gets the type of the result of the operation.
        /// </summary>
        OperationResultType ResultType { get; }

        /// <summary>
        /// When the result is a failure, gets the error 
        /// that caused the operation to fail.
        /// </summary>
        IOperationError? Error { get; }
    }
}
