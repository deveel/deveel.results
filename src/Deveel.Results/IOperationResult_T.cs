namespace Deveel
{
    /// <summary>
    /// Represents the result of an operation that
    /// has a value returned.
    /// </summary>
    /// <typeparam name="T">The type of the value returned by the operation.</typeparam>
    public interface IOperationResult<T> : IOperationResult
    {
        /// <summary>
        /// Gets the value returned by an operation.
        /// </summary>
        T? Value { get; }
    }
}
