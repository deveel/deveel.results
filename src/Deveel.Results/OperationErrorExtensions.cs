namespace Deveel
{
    /// <summary>
    /// Extends the <see cref="IOperationError"/> contract to provide
    /// additional functionality.
    /// </summary>
    public static class OperationErrorExtensions
    {
        /// <summary>
        /// Converts the error to an operation exception.
        /// </summary>
        /// <param name="error">
        /// The error to convert to an exception.
        /// </param>
        /// <returns>
        /// Returns an instance of <see cref="OperationException"/> that
        /// represents the error.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="error"/> is <see langword="null"/>.
        /// </exception>
        public static OperationException AsException(this IOperationError error)
        {
            ArgumentNullException.ThrowIfNull(error, nameof(error));
            return new OperationException(error.Code, error.Domain, error.Message, error.InnerError?.AsException());
        }
    }
}
