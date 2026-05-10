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
		/// Determines if the operation result is a success and has a value.
		/// </summary>
		/// <typeparam name="T">
		/// The type of the value that is expected to be returned by the operation.
		/// </typeparam>
		/// <param name="result">
		/// The operation result to check.
		/// </param>
		/// <returns>
		/// Returns <see langword="true"/> if the operation result is a success
		/// and the value is not <see langword="null"/>, otherwise <see langword="false"/>.
		/// </returns>
		public static bool HasValue<T>(this IOperationResult<T> result)
			=> result.IsSuccess() && result.Value is not null;

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
        /// Executes the given function if the operation result is a success,
        /// binding the result of the function to the current result chain.
        /// </summary>
        /// <param name="result">
        /// The operation result to evaluate.
        /// </param>
        /// <param name="func">
        /// A function to invoke when the operation result is a success.
        /// </param>
        /// <returns>
        /// Returns the <see cref="IOperationResult"/> produced by <paramref name="func"/>
        /// if the current result is a success; otherwise returns the current (failed) result.
        /// If an exception implementing <see cref="IOperationError"/> is thrown it is wrapped
        /// in a failed result; any other unhandled exception is also wrapped as a failure.
        /// </returns>
        public static IOperationResult Bind(this IOperationResult result, Func<IOperationResult> func)
        {
            Check.ThrowIfNull(result, nameof(result));
            Check.ThrowIfNull(func, nameof(func));

            try
            {
                if (result.IsSuccess())
                    return func();

                return result;
            }
            catch (Exception ex) when (ex is IOperationError)
            {
                return OperationResult.Fail((IOperationError) ex);
            }
            catch (Exception ex)
            {
                return OperationResult.Fail(
                    "UnhandledException", 
                    "RESULT", 
                    "An unhandled exception occurred while executing the operation.",
                    ex.AsOperationError());
            }
        }
        
        /// <summary>
        /// Asynchronously executes the given function if the operation result is a success,
        /// binding the result of the function to the current result chain.
        /// </summary>
        /// <param name="result">
        /// The operation result to evaluate.
        /// </param>
        /// <param name="func">
        /// An asynchronous function to invoke when the operation result is a success.
        /// </param>
        /// <returns>
        /// Returns a <see cref="Task{TResult}"/> that resolves to the <see cref="IOperationResult"/>
        /// produced by <paramref name="func"/> if the current result is a success; otherwise
        /// resolves to the current (failed) result.
        /// If an exception implementing <see cref="IOperationError"/> is thrown it is wrapped
        /// in a failed result; any other unhandled exception is also wrapped as a failure.
        /// </returns>
        public static async Task<IOperationResult> BindAsync(this IOperationResult result, Func<Task<IOperationResult>> func)
        {
            Check.ThrowIfNull(result, nameof(result));
            Check.ThrowIfNull(func, nameof(func));

            try
            {
                if (result.IsSuccess())
                    return await func().ConfigureAwait(false);

                return result;
            }
            catch (Exception ex) when (ex is IOperationError)
            {
                return OperationResult.Fail((IOperationError) ex);
            }
            catch (Exception ex)
            {
                return OperationResult.Fail(
                    "UnhandledException", 
                    "RESULT", 
                    "An unhandled exception occurred while executing the operation.",
                    ex.AsOperationError());
            }
        }
        
        /// <summary>
        /// Executes the given function if the operation result is a success,
        /// binding the typed result of the function to the current result chain.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the value that is expected to be returned by the next operation.
        /// </typeparam>
        /// <param name="result">
        /// The operation result to evaluate.
        /// </param>
        /// <param name="func">
        /// A function to invoke when the operation result is a success.
        /// </param>
        /// <returns>
        /// Returns the <see cref="IOperationResult{T}"/> produced by <paramref name="func"/>
        /// if the current result is a success; otherwise returns a failed <see cref="IOperationResult{T}"/>
        /// carrying the original error.
        /// If an exception implementing <see cref="IOperationError"/> is thrown it is wrapped
        /// in a failed result; any other unhandled exception is also wrapped as a failure.
        /// </returns>
        public static IOperationResult<T> Bind<T>(this IOperationResult result, Func<IOperationResult<T>> func)
        {
            Check.ThrowIfNull(result, nameof(result));
            Check.ThrowIfNull(func, nameof(func));

            try
            {
                if (result.IsSuccess())
                    return func();

                return OperationResult<T>.Fail(result.Error!);
            }
            catch (Exception ex) when (ex is IOperationError)
            {
                return OperationResult<T>.Fail((IOperationError) ex);
            }
            catch (Exception ex)
            {
                return OperationResult<T>.Fail(
                    "UnhandledException", 
                    "RESULT", 
                    "An unhandled exception occurred while executing the operation.",
                    ex.AsOperationError());
            }
        }
        
        /// <summary>
        /// Asynchronously executes the given function if the operation result is a success,
        /// binding the typed result of the function to the current result chain.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the value that is expected to be returned by the next operation.
        /// </typeparam>
        /// <param name="result">
        /// The operation result to evaluate.
        /// </param>
        /// <param name="func">
        /// An asynchronous function to invoke when the operation result is a success.
        /// </param>
        /// <returns>
        /// Returns a <see cref="Task{TResult}"/> that resolves to the <see cref="IOperationResult{T}"/>
        /// produced by <paramref name="func"/> if the current result is a success; otherwise resolves to
        /// a failed <see cref="IOperationResult{T}"/> carrying the original error.
        /// If an exception implementing <see cref="IOperationError"/> is thrown it is wrapped
        /// in a failed result; any other unhandled exception is also wrapped as a failure.
        /// </returns>
        public static async Task<IOperationResult<T>> BindAsync<T>(this IOperationResult result, Func<Task<IOperationResult<T>>> func)
        {
            Check.ThrowIfNull(result, nameof(result));
            Check.ThrowIfNull(func, nameof(func));

            try
            {
                if (result.IsSuccess())
                    return await func().ConfigureAwait(false);

                return OperationResult<T>.Fail(result.Error!);
            }
            catch (Exception ex) when (ex is IOperationError)
            {
                return OperationResult<T>.Fail((IOperationError) ex);
            }
            catch (Exception ex)
            {
                return OperationResult<T>.Fail(
                    "UnhandledException", 
                    "RESULT", 
                    "An unhandled exception occurred while executing the operation.",
                    ex.AsOperationError());
            }
        }
        
        /// <summary>
        /// Executes the given function with the value carried by the operation result
        /// if the result is a success, binding the outcome to the current result chain.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the value carried by the operation result.
        /// </typeparam>
        /// <param name="result">
        /// The typed operation result to evaluate.
        /// </param>
        /// <param name="func">
        /// A function that receives the value of the current result and produces
        /// the next <see cref="IOperationResult"/> in the chain.
        /// </param>
        /// <returns>
        /// Returns the <see cref="IOperationResult"/> produced by <paramref name="func"/>
        /// if the current result is a success; otherwise returns the current (failed) result.
        /// If an exception implementing <see cref="IOperationError"/> is thrown it is wrapped
        /// in a failed result; any other unhandled exception is also wrapped as a failure.
        /// </returns>
        public static IOperationResult Bind<T>(this IOperationResult<T> result, Func<T?, IOperationResult> func)
        {
            Check.ThrowIfNull(result, nameof(result));
            Check.ThrowIfNull(func, nameof(func));

            try
            {
                if (result.IsSuccess())
                    return func(result.Value);

                return result;
            }
            catch (Exception ex) when (ex is IOperationError)
            {
                return OperationResult.Fail((IOperationError) ex);
            }
            catch (Exception ex)
            {
                return OperationResult.Fail(
                    "UnhandledException", 
                    "RESULT", 
                    "An unhandled exception occurred while executing the operation.",
                    ex.AsOperationError());
            }
        }
        
        /// <summary>
        /// Asynchronously executes the given function with the value carried by the operation result
        /// if the result is a success, binding the outcome to the current result chain.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the value carried by the operation result.
        /// </typeparam>
        /// <param name="result">
        /// The typed operation result to evaluate.
        /// </param>
        /// <param name="func">
        /// An asynchronous function that receives the value of the current result and produces
        /// the next <see cref="IOperationResult"/> in the chain.
        /// </param>
        /// <returns>
        /// Returns a <see cref="Task{TResult}"/> that resolves to the <see cref="IOperationResult"/>
        /// produced by <paramref name="func"/> if the current result is a success; otherwise resolves
        /// to the current (failed) result.
        /// If an exception implementing <see cref="IOperationError"/> is thrown it is wrapped
        /// in a failed result; any other unhandled exception is also wrapped as a failure.
        /// </returns>
        public static async Task<IOperationResult> BindAsync<T>(this IOperationResult<T> result, Func<T?, Task<IOperationResult>> func)
        {
            Check.ThrowIfNull(result, nameof(result));
            Check.ThrowIfNull(func, nameof(func));

            try
            {
                if (result.IsSuccess())
                    return await func(result.Value).ConfigureAwait(false);

                return result;
            }
            catch (Exception ex) when (ex is IOperationError)
            {
                return OperationResult.Fail((IOperationError) ex);
            }
            catch (Exception ex)
            {
                return OperationResult.Fail(
                    "UnhandledException", 
                    "RESULT", 
                    "An unhandled exception occurred while executing the operation.",
                    ex.AsOperationError());
            }
        }
        
        /// <summary>
        /// Executes a side-effect action on the operation result without altering the result,
        /// and then returns the same result.
        /// </summary>
        /// <param name="result">
        /// The operation result to pass to the action.
        /// </param>
        /// <param name="action">
        /// An action to invoke with the current operation result, regardless of its state.
        /// </param>
        /// <returns>
        /// Returns the same <see cref="IOperationResult"/> that was passed in, allowing further
        /// chaining. If the action throws an exception implementing <see cref="IOperationError"/>
        /// it is wrapped in a failed result; any other unhandled exception is also wrapped as a failure.
        /// </returns>
        public static IOperationResult Tap(this IOperationResult result, Action<IOperationResult> action)
        {
            Check.ThrowIfNull(result, nameof(result));
            Check.ThrowIfNull(action, nameof(action));

            try
            {
                action(result);
                return result;
            }
            catch (Exception ex) when(ex is IOperationError)
            {
                return OperationResult.Fail((IOperationError) ex);
            }
            catch (Exception ex)
            {
                return OperationResult.Fail(
                    "UnhandledException", 
                    "RESULT", 
                    "An unhandled exception occurred while executing the operation.",
                    ex.AsOperationError());
            }
        }
        
        /// <summary>
        /// Asynchronously executes a side-effect action on the operation result without altering
        /// the result, and then returns the same result.
        /// </summary>
        /// <param name="result">
        /// The operation result to pass to the action.
        /// </param>
        /// <param name="action">
        /// An asynchronous action to invoke with the current operation result, regardless of its state.
        /// </param>
        /// <returns>
        /// Returns a <see cref="Task{TResult}"/> that resolves to the same <see cref="IOperationResult"/>
        /// that was passed in, allowing further chaining. If the action throws an exception implementing
        /// <see cref="IOperationError"/> it is wrapped in a failed result; any other unhandled exception
        /// is also wrapped as a failure.
        /// </returns>
        public static async Task<IOperationResult> TapAsync(this IOperationResult result, Func<IOperationResult, Task> action)
        {
            Check.ThrowIfNull(result, nameof(result));
            Check.ThrowIfNull(action, nameof(action));

            try
            {
                await action(result).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex) when(ex is IOperationError)
            {
                return OperationResult.Fail((IOperationError) ex);
            }
            catch (Exception ex)
            {
                return OperationResult.Fail(
                    "UnhandledException", 
                    "RESULT", 
                    "An unhandled exception occurred while executing the operation.",
                    ex.AsOperationError());
            }
        }
        
        /// <summary>
        /// Executes a side-effect action on the typed operation result without altering the result,
        /// and then returns the same result.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the value carried by the operation result.
        /// </typeparam>
        /// <param name="result">
        /// The typed operation result to pass to the action.
        /// </param>
        /// <param name="action">
        /// An action to invoke with the current typed operation result, regardless of its state.
        /// </param>
        /// <returns>
        /// Returns the same <see cref="IOperationResult{T}"/> that was passed in, allowing further
        /// chaining. If the action throws an exception implementing <see cref="IOperationError"/>
        /// it is wrapped in a failed result; any other unhandled exception is also wrapped as a failure.
        /// </returns>
        public static IOperationResult<T> Tap<T>(this IOperationResult<T> result, Action<IOperationResult<T>> action)
        {
            Check.ThrowIfNull(result, nameof(result));
            Check.ThrowIfNull(action, nameof(action));

            try
            {
                action(result);
                return result;
            }
            catch (Exception ex) when(ex is IOperationError)
            {
                return OperationResult<T>.Fail((IOperationError) ex);
            }
            catch (Exception ex)
            {
                return OperationResult<T>.Fail(
                    "UnhandledException", 
                    "RESULT", 
                    "An unhandled exception occurred while executing the operation.",
                    ex.AsOperationError());
            }
        }
        
        /// <summary>
        /// Asynchronously executes a side-effect action on the typed operation result without
        /// altering the result, and then returns the same result.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the value carried by the operation result.
        /// </typeparam>
        /// <param name="result">
        /// The typed operation result to pass to the action.
        /// </param>
        /// <param name="action">
        /// An asynchronous action to invoke with the current typed operation result, regardless of its state.
        /// </param>
        /// <returns>
        /// Returns a <see cref="Task{TResult}"/> that resolves to the same <see cref="IOperationResult{T}"/>
        /// that was passed in, allowing further chaining. If the action throws an exception implementing
        /// <see cref="IOperationError"/> it is wrapped in a failed result; any other unhandled exception
        /// is also wrapped as a failure.
        /// </returns>
        public static async Task<IOperationResult<T>> TapAsync<T>(this IOperationResult<T> result, Func<IOperationResult<T>, Task> action)
        {
            Check.ThrowIfNull(result, nameof(result));
            Check.ThrowIfNull(action, nameof(action));

            try
            {
                await action(result).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex) when(ex is IOperationError)
            {
                return OperationResult<T>.Fail((IOperationError) ex);
            }
            catch (Exception ex)
            {
                return OperationResult<T>.Fail(
                    "UnhandledException", 
                    "RESULT", 
                    "An unhandled exception occurred while executing the operation.",
                    ex.AsOperationError());
            }
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
        /// A function that is called when the operation result caused no changes
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
            Check.ThrowIfNull(result, nameof(result));

            if (result.IsSuccess())
            {
                Check.ThrowIfNull(ifSuccess, nameof(ifSuccess));
                return ifSuccess();
            }

            if (result.IsError())
            {
                Check.ThrowIfNull(ifError, nameof(ifError));
                return ifError(result.Error);
            }

            if (result.IsUnchanged())
            {
                Check.ThrowIfNull(ifUnchanged, nameof(ifUnchanged));
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
        /// A function that is called when the operation result caused no changes
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
            Check.ThrowIfNull(result, nameof(result));

            if (result.IsSuccess())
            {
                Check.ThrowIfNull(ifSuccess, nameof(ifSuccess));
                return ifSuccess();
            }

            if (result.IsError())
            {
                Check.ThrowIfNull(ifError, nameof(ifError));
                return ifError(result.Error);
            }

            if (result.IsUnchanged())
            {
                Check.ThrowIfNull(ifUnchanged, nameof(ifUnchanged));
                return ifUnchanged();
            }

            throw new InvalidOperationException("The operation result is in an unknown state.");
        }

        /// <summary>
        /// Attempts to match the operation result to a specific state
        /// that can be handled by the caller.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the value that is expected to be returned by the operation.
        /// </typeparam>
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
        /// A function that is called when the operation result caused no changes
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
            Check.ThrowIfNull(result, nameof(result));

            if (result.IsSuccess())
            {
                Check.ThrowIfNull(ifSuccess, nameof(ifSuccess));
                return ifSuccess(result.Value);
            }

            if (result.IsError())
            {
                Check.ThrowIfNull(ifError, nameof(ifError));
                return ifError(result.Error);
            }

            if (result.IsUnchanged())
            {
                Check.ThrowIfNull(ifUnchanged, nameof(ifUnchanged));
                return ifUnchanged(result.Value);
            }

            throw new InvalidOperationException("The operation result is in an unknown state.");
        }

        /// <summary>
        /// Attempts to match the operation result to a specific state
        /// that can be handled by the caller.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the value that is expected to be returned by the operation.
        /// </typeparam>
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
        /// A function that is called when the operation result caused no changes
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
            Check.ThrowIfNull(result, nameof(result));

            if (result.IsSuccess())
            {
                Check.ThrowIfNull(ifSuccess, nameof(ifSuccess));
                return ifSuccess(result.Value);
            }

            if (result.IsError())
            {
                Check.ThrowIfNull(ifError, nameof(ifError));
                return ifError(result.Error);
            }

            if (result.IsUnchanged())
            {
                Check.ThrowIfNull(ifUnchanged, nameof(ifUnchanged));
                return ifUnchanged(result.Value);
            }

            throw new InvalidOperationException("The operation result is in an unknown state.");
        }



        /// <summary>
        /// Converts an error result to an exception.
        /// </summary>
        /// <param name="result">
        /// The operation result to convert to an exception.
        /// </param>
        /// <returns>
        /// Returns an instance of <see cref="OperationException"/> if the result is an error,
        /// otherwise <see langword="null"/>.
        /// </returns>
        public static OperationException? AsException(this IOperationResult result)
        {
            if (!result.IsError() || result.Error == null)
                return null;

            return result.Error.AsException();
        }
    }
}
