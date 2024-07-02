namespace Deveel
{
    /// <summary>
    /// Enumerates the possible types of an operation result.
    /// </summary>
    public enum OperationResultType
    {
        /// <summary>
        /// The type of the operation result is unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The operation was successful.
        /// </summary>
        Success = 1,

        /// <summary>
        /// The operation failed.
        /// </summary>
        Error = 2,

        /// <summary>
        /// The operation caused no changes.
        /// </summary>
        Unchanged = 3,

        /// <summary>
        /// The operation was cancelled.
        /// </summary>
        Cancelled = 4
    }
}
