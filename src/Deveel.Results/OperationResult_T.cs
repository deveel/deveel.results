namespace Deveel
{
    public readonly struct OperationResult<T> : IOperationResult<T>
    {
        private OperationResult(OperationResultType resultType, T? value, IOperationError? error)
        {
            ResultType = resultType;
            Value = value;
            Error = error;
        }

        /// <inheritdoc/>
        public T? Value { get; }

        /// <inheritdoc/>
        public OperationResultType ResultType { get; }

        /// <inheritdoc/>
        public IOperationError? Error { get; }

        /// <summary>
        /// A result of an operation that has not changed the state 
        /// of an object.
        /// </summary>
        public static readonly OperationResult<T> NotChanged = new(OperationResultType.Unchanged, default, null);

        /// <summary>
        /// A result of an operation that has been cancelled.
        /// </summary>
        public static readonly OperationResult<T> Cancelled = new(OperationResultType.Cancelled, default, null);

        /// <summary>
        /// Creates a new instance of an operation result that has succeeded
        /// with the given value.
        /// </summary>
        /// <param name="value">
        /// A value that represents the result of the operation.
        /// </param>
        /// <returns></returns>
        public static OperationResult<T> Success(T? value)
        {
            return new OperationResult<T>(OperationResultType.Success, value, null);
        }

        public static OperationResult<T> Fail(IOperationError error)
        {
            ArgumentNullException.ThrowIfNull(error, nameof(error));
            return new OperationResult<T>(OperationResultType.Error, default, error);
        }

        public static OperationResult<T> Fail(string code, string domain, string? message = null, IOperationError? inner = null)
        {
            ArgumentNullException.ThrowIfNull(code, nameof(code));
            ArgumentNullException.ThrowIfNull(domain, nameof(domain));
            return new OperationResult<T>(OperationResultType.Error, default, new OperationError(code, domain, message, inner));
        }

        public static implicit operator OperationResult<T>(OperationResult result)
            => new OperationResult<T>(result.ResultType, default, result.Error);

        public static implicit operator OperationResult<T>(T value) => Success(value);

        public static implicit operator OperationResult<T>(OperationException error) => Fail(error);
    }
}
