namespace Deveel
{
    /// <summary>
    /// Extensions for the <see cref="IOperationResult"/> contract.
    /// </summary>
    public static class OperationResultExtensions
    {
        public static bool IsSuccess(this IOperationResult result) 
            => result.ResultType == OperationResultType.Success;

        public static bool IsError(this IOperationResult result)
            => result.ResultType == OperationResultType.Error;

        public static bool IsCancelled(this IOperationResult result)
            => result.ResultType == OperationResultType.Cancelled;

        public static bool IsUnchanged(this IOperationResult result)
            => result.ResultType == OperationResultType.Unchanged;

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
    }
}
