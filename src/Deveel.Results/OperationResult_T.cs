using System.ComponentModel.DataAnnotations;

namespace Deveel
{
    /// <summary>
    /// A result of an operation that has a value of a specific type.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the value that represents the result of the operation.
    /// </typeparam>
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

        /// <summary>
        /// Creates a new instance of an operation result that has failed
        /// because of an error.
        /// </summary>
        /// <param name="error">
        /// The error that caused the operation to fail.
        /// </param>
        /// <returns>
        /// Returns an instance of <see cref="OperationResult{T}"/> that
        /// represents a failure in the operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="error"/> is <see langword="null"/>.
        /// </exception>
        public static OperationResult<T> Fail(IOperationError error)
        {
            ArgumentNullException.ThrowIfNull(error, nameof(error));
            return new OperationResult<T>(OperationResultType.Error, default, error);
        }

        /// <summary>
        /// Creates a new instance of an operation result that has failed
        /// because of an error.
        /// </summary>
        /// <param name="code">
        /// The code of the error that caused the operation to fail.
        /// </param>
        /// <param name="domain">
        /// The domain where the error occurred.
        /// </param>
        /// <param name="message">
        /// A message that describes the error.
        /// </param>
        /// <param name="inner">
        /// An inner error that caused the error.
        /// </param>
        /// <returns>
        /// Returns an instance of <see cref="OperationResult{T}"/> that
        /// represents a failure in the operation.
        /// </returns>
        public static OperationResult<T> Fail(string code, string domain, string? message = null, IOperationError? inner = null)
        {
            ArgumentNullException.ThrowIfNull(code, nameof(code));
            ArgumentNullException.ThrowIfNull(domain, nameof(domain));
            return new OperationResult<T>(OperationResultType.Error, default, new OperationError(code, domain, message, inner));
        }

        /// <summary>
        /// Creates a new instance of an operation result that has failed
        /// because of a validation error.
        /// </summary>
        /// <param name="code">
        /// The code of the error that caused the operation to fail.
        /// </param>
        /// <param name="domain">
        /// The domain where the error occurred.
        /// </param>
        /// <param name="results">
        /// The list of validation results that caused the error.
        /// </param>
        /// <returns>
        /// Returns an instance of <see cref="OperationResult{T}"/> that
        /// represents a failure in the operation.
        /// </returns>
        public static OperationResult<T> ValidationFailed(string code, string domain, IEnumerable<ValidationResult> results)
            => Fail(new OperationValidationError(code, domain, results.ToList()));

        /// <summary>
        /// Implicitly converts an instance of <see cref="OperationResult{T}"/>
        /// to an instance of <see cref="OperationResult"/>.
        /// </summary>
        /// <remarks>
        /// This conversion is useful when you need to convert an operation result
        /// to a strongly-typed result, for example, in contexts where the expected
        /// result of a method is strongly-typed, but a child operation may return
        /// a generic result, that is an error, and you want to propagate the error
        /// without losing the type information.
        /// </remarks>
        /// <param name="result">
        /// The operation result to convert.
        /// </param>
        public static implicit operator OperationResult<T>(OperationResult result)
            => new OperationResult<T>(result.ResultType, default, result.Error);

        /// <summary>
        /// Implicitly converts an instance of <typeparamref name="T"/> to an
        /// operation result that represents a success.
        /// </summary>
        /// <param name="value">
        /// The value to convert to an operation result.
        /// </param>
        /// <seealso cref="Success"/>
        public static implicit operator OperationResult<T>(T value) => Success(value);

        /// <summary>
        /// Implicitly converts an instance of <see cref="OperationException"/>
        /// to an operation result that represents a failure.
        /// </summary>
        /// <param name="error">
        /// The error that caused the operation to fail.
        /// </param>
        /// <seealso cref="Fail(IOperationError)"/>
        public static implicit operator OperationResult<T>(OperationException error) => Fail(error);
    }
}
