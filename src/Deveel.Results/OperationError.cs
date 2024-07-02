namespace Deveel
{
    /// <summary>
    /// Implements a simple error object that can be used to represent
    /// a failure in an operation.
    /// </summary>
    public readonly struct OperationError : IOperationError
    {
        /// <summary>
        /// Constructs an instance of an <see cref="OperationError"/> object.
        /// </summary>
        /// <param name="code">
        /// The code of the error, that is unique within the 
        /// given domain.
        /// </param>
        /// <param name="domain">
        /// The domain where the error occurred.
        /// </param>
        /// <param name="message">
        /// A message that describes the error.
        /// </param>
        /// <param name="innerError">
        /// A reference to an inner error that caused this error.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="code"/> or <paramref name="domain"/>
        /// are <see langword="null"/>.
        /// </exception>
        public OperationError(string code, string domain, string? message = null, IOperationError? innerError = null)
        {
            ArgumentNullException.ThrowIfNull(code, nameof(code));
            ArgumentNullException.ThrowIfNull(domain, nameof(domain));

            Code = code;
            Domain = domain;
            Message = message;
            InnerError = innerError;
        }

        /// <inheritdoc />
        public string Code { get; }

        /// <inheritdoc />
        public string Domain { get; }

        /// <inheritdoc />
        public string? Message { get; }

        /// <inheritdoc />
        public IOperationError? InnerError { get; }
    }
}
