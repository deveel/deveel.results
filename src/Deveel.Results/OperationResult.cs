namespace Deveel
{
    /// <summary>
    /// An operation result that has no value returned.
    /// </summary>
    public readonly struct OperationResult : IOperationResult
    {
        private OperationResult(OperationResultType resultType, IOperationError? error)
        {
            ResultType = resultType;
            Error = error;
        }

        /// <inheritdoc/>
        public OperationResultType ResultType { get; }

        /// <inheritdoc/>
        public IOperationError? Error { get; }

        /// <summary>
        /// The result of a successful operation.
        /// </summary>
        public static readonly OperationResult Success = new(OperationResultType.Success, null);

        /// <summary>
        /// The result of an operation that has not changed the state 
        /// of an object.
        /// </summary>
        public static readonly OperationResult NotChanged = new(OperationResultType.Unchanged, null);

        /// <summary>
        /// The result of an operation that has been cancelled.
        /// </summary>
        public static readonly OperationResult Cancelled = new(OperationResultType.Cancelled, null);

        /// <summary>
        /// Creates a new instance of an operation result that has failed.
        /// </summary>
        /// <param name="error">
        /// The error that has caused the operation to fail.
        /// </param>
        /// <returns>
        /// Returns an instance of <see cref="OperationResult"/> that represents
        /// a failed operation.
        /// </returns>
        public static OperationResult Fail(IOperationError error)
        {
            ArgumentNullException.ThrowIfNull(error, nameof(error));
            return new OperationResult(OperationResultType.Error, error);
        }

        /// <summary>
        /// Creates a new instance of an operation result that has failed.
        /// </summary>
        /// <param name="code">
        /// The code of the error that has caused the operation to fail.
        /// </param>
        /// <param name="domain">
        /// The domain where the error has occurred.
        /// </param>
        /// <param name="message">
        /// A message that describes the error.
        /// </param>
        /// <param name="inner">
        /// A nested error that has caused the operation to fail.
        /// </param>
        /// <returns>
        /// Returns an instance of <see cref="OperationResult"/> that represents
        /// a failed operation.
        /// </returns>
        public static OperationResult Fail(string code, string domain, string? message = null, IOperationError? inner = null)
            => Fail(new OperationError(code, domain, message, inner));

        /// <summary>
        /// Implicitly converts an instance of an error to an operation result.
        /// </summary>
        /// <param name="error">
        /// The error that has caused the operation to fail.
        /// </param>
        public static implicit operator OperationResult(OperationError error) => Fail(error);

        /// <summary>
        /// Implicitly converts an instance of an operation error to an operation result.
        /// </summary>
        /// <param name="error">
        /// The error that has caused the operation to fail.
        /// </param>
        public static implicit operator OperationResult(OperationException error) => Fail(error);
    }
}
