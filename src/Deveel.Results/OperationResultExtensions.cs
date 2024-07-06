using System.ComponentModel.DataAnnotations;

namespace Deveel
{
    /// <summary>
    /// Extensions for the <see cref="IOperationResult"/> contract.
    /// </summary>
    public static class OperationResultExtensions
    {
        /// <summary>
        /// Determines if the operation result is a success.
        /// </summary>
        /// <param name="result">
        /// The operation result to check.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if the operation result is a success,
        /// otherwise <see langword="false"/>.
        /// </returns>
        public static bool IsSuccess(this IOperationResult result) 
            => result.ResultType == OperationResultType.Success;

        /// <summary>
        /// Determines if the operation result is an error.
        /// </summary>
        /// <param name="result">
        /// The operation result to check.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if the operation result is an error,
        /// otherwise <see langword="false"/>.
        /// </returns>
        public static bool IsError(this IOperationResult result)
            => result.ResultType == OperationResultType.Error;

        /// <summary>
        /// Determines if the operation has caused no changes
        /// to the state of an object.
        /// </summary>
        /// <param name="result">
        /// The operation result to check.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if the operation has not
        /// caused any changes to the object, otherwise <see langword="false"/>.
        /// </returns>
        public static bool IsUnchanged(this IOperationResult result)
            => result.ResultType == OperationResultType.Unchanged;

        /// <summary>
        /// Checks if the operation result has validation errors.
        /// </summary>
        /// <param name="result">
        /// The operation result to check.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if the operation result is
        /// an error and the type of the error is <see cref="IValidationError"/>,
        /// otherwise <see langword="false"/>.
        /// </returns>
        public static bool HasValidationErrors(this IOperationResult result)
            => result.IsError() && (result.Error is IValidationError);

        /// <summary>
        /// Gets the list of validation results that caused an operation
        /// to fail.
        /// </summary>
        /// <param name="result">
        /// The result that contains the validation errors.
        /// </param>
        /// <returns>
        /// Returns a list of <see cref="ValidationResult"/> objects that
        /// describe the validation errors that caused the operation to fail.
        /// </returns>
        public static IReadOnlyList<ValidationResult> ValidationResults(this IOperationResult result) 
            => result.HasValidationErrors() ? ((IValidationError)result.Error!).ValidationResults : Array.Empty<ValidationResult>();

        /// <summary>
        /// Attempts to match the operation result to a specific state
        /// that can be handled by the caller.
        /// </summary>
        /// <typeparam name="TResult">
        /// The type of the result that is returned by the match.
        /// </typeparam>
        /// <param name="result">
        /// The operation result to match.
        /// </param>
        /// <param name="ifSuccess">
        /// A function that is called when the operation result was a success.
        /// </param>
        /// <param name="ifError">
        /// A function that is called when the operation result was an error.
        /// </param>
        /// <param name="ifUnchanged">
        /// A function that is called when the operation result caused no changed
        /// to the object.
        /// </param>
        /// <returns>
        /// Returns the result of the function that was called based on the state
        /// of the operation result.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the operation result is in an unknown state.
        /// </exception>
        public static TResult Match<TResult>(this IOperationResult result, 
            Func<TResult>? ifSuccess = null, 
            Func<IOperationError?, TResult>? ifError = null, 
            Func<TResult>? ifUnchanged = null)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));

            if (result.IsSuccess())
            {
                ArgumentNullException.ThrowIfNull(ifSuccess, nameof(ifSuccess));
                return ifSuccess();
            }

            if (result.IsError())
            {
                ArgumentNullException.ThrowIfNull(ifError, nameof(ifError));
                return ifError(result.Error);
            }

            if (result.IsUnchanged())
            {
                ArgumentNullException.ThrowIfNull(ifUnchanged, nameof(ifUnchanged));
                return ifUnchanged();
            }

            throw new InvalidOperationException("The operation result is in an unknown state.");
        }

        /// <summary>
        /// Attempts to match the operation result to a specific state
        /// that can be handled by the caller.
        /// </summary>
        /// <typeparam name="TResult">
        /// The type of the result that is returned by the match.
        /// </typeparam>
        /// <param name="result">
        /// The operation result to match.
        /// </param>
        /// <param name="ifSuccess">
        /// A function that is called when the operation result was a success.
        /// </param>
        /// <param name="ifError">
        /// A function that is called when the operation result was an error.
        /// </param>
        /// <param name="ifUnchanged">
        /// A function that is called when the operation result caused no changed
        /// to the object.
        /// </param>
        /// <returns>
        /// Returns the result of the function that was called based on the state
        /// of the operation result.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the operation result is in an unknown state.
        /// </exception>
        public static Task<TResult> MatchAsync<TResult>(this IOperationResult result,
            Func<Task<TResult>>? ifSuccess = null,
            Func<IOperationError?, Task<TResult>>? ifError = null,
            Func<Task<TResult>>? ifUnchanged = null)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));

            if (result.IsSuccess())
            {
                ArgumentNullException.ThrowIfNull(ifSuccess, nameof(ifSuccess));
                return ifSuccess();
            }

            if (result.IsError())
            {
                ArgumentNullException.ThrowIfNull(ifError, nameof(ifError));
                return ifError(result.Error);
            }

            if (result.IsUnchanged())
            {
                ArgumentNullException.ThrowIfNull(ifUnchanged, nameof(ifUnchanged));
                return ifUnchanged();
            }

            throw new InvalidOperationException("The operation result is in an unknown state.");
        }

        /// <summary>
        /// Attempts to match the operation result to a specific state
        /// that can be handled by the caller.
        /// </summary>
        /// <typeparam name="TResult">
        /// The type of the result that is returned by the match.
        /// </typeparam>
        /// <param name="result">
        /// The operation result to match.
        /// </param>
        /// <param name="ifSuccess">
        /// A function that is called when the operation result was a success.
        /// </param>
        /// <param name="ifError">
        /// A function that is called when the operation result was an error.
        /// </param>
        /// <param name="ifUnchanged">
        /// A function that is called when the operation result caused no changed
        /// to the object.
        /// </param>
        /// <returns>
        /// Returns the result of the function that was called based on the state
        /// of the operation result.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the operation result is in an unknown state.
        /// </exception>
        public static TResult Match<T, TResult>(this IOperationResult<T> result,
            Func<T?, TResult>? ifSuccess = null,
            Func<IOperationError?, TResult>? ifError = null,
            Func<T?, TResult>? ifUnchanged = null)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));

            if (result.IsSuccess())
            {
                ArgumentNullException.ThrowIfNull(ifSuccess, nameof(ifSuccess));
                return ifSuccess(result.Value);
            }

            if (result.IsError())
            {
                ArgumentNullException.ThrowIfNull(ifError, nameof(ifError));
                return ifError(result.Error);
            }

            if (result.IsUnchanged())
            {
                ArgumentNullException.ThrowIfNull(ifUnchanged, nameof(ifUnchanged));
                return ifUnchanged(result.Value);
            }

            throw new InvalidOperationException("The operation result is in an unknown state.");
        }

        /// <summary>
        /// Attempts to match the operation result to a specific state
        /// that can be handled by the caller.
        /// </summary>
        /// <typeparam name="TResult">
        /// The type of the result that is returned by the match.
        /// </typeparam>
        /// <param name="result">
        /// The operation result to match.
        /// </param>
        /// <param name="ifSuccess">
        /// A function that is called when the operation result was a success.
        /// </param>
        /// <param name="ifError">
        /// A function that is called when the operation result was an error.
        /// </param>
        /// <param name="ifUnchanged">
        /// A function that is called when the operation result caused no changed
        /// to the object.
        /// </param>
        /// <returns>
        /// Returns the result of the function that was called based on the state
        /// of the operation result.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the operation result is in an unknown state.
        /// </exception>
        public static Task<TResult> MatchAsync<T, TResult>(this IOperationResult<T> result,
            Func<T?, Task<TResult>>? ifSuccess = null,
            Func<IOperationError?, Task<TResult>>? ifError = null,
            Func<T?, Task<TResult>>? ifUnchanged = null)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));

            if (result.IsSuccess())
            {
                ArgumentNullException.ThrowIfNull(ifSuccess, nameof(ifSuccess));
                return ifSuccess(result.Value);
            }

            if (result.IsError())
            {
                ArgumentNullException.ThrowIfNull(ifError, nameof(ifError));
                return ifError(result.Error);
            }

            if (result.IsUnchanged())
            {
                ArgumentNullException.ThrowIfNull(ifUnchanged, nameof(ifUnchanged));
                return ifUnchanged(result.Value);
            }

            throw new InvalidOperationException("The operation result is in an unknown state.");
        }



        /// <summary>
        /// Converts an error result to an exception.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static OperationException? AsException(this IOperationResult result)
        {
            if (!result.IsError() || result.Error == null)
                return null;

            return result.Error.AsException();
        }
    }
}
