// Non-nullable field must contain a non-null value when exiting constructor.
#pragma warning disable CS8618

namespace Deveel
{
    /// <summary>
    /// An exception that represents an unhandled error that has 
    /// occurred during an operation.
    /// </summary>
    public class OperationException : Exception, IOperationError
    {
        /// <summary>
        /// Constructs an instance of an <see cref="OperationException"/> with
        /// the specified error code and domain.
        /// </summary>
        /// <param name="errorCode">
        /// The unique code that identifies the error within the domain.
        /// </param>
        /// <param name="errorDomain">
        /// The domain where the error has occurred.
        /// </param>
        public OperationException(string errorCode, string errorDomain)
        {
            SetErrorDetails(errorCode, errorDomain);
        }

        /// <summary>
        /// Constructs an instance of an <see cref="OperationException"/> with
        /// the specified error code and domain.
        /// </summary>
        /// <param name="errorCode">
        /// The unique code that identifies the error within the domain.
        /// </param>
        /// <param name="errorDomain">
        /// The domain where the error has occurred.
        /// </param>
        /// <param name="message">
        /// A message that describes the error.
        /// </param>
        public OperationException(string errorCode, string errorDomain, string? message)
            : base(message)
        {
            SetErrorDetails(errorCode, errorDomain);
        }

        /// <summary>
        /// Constructs an instance of an <see cref="OperationException"/> with
        /// the specified error code and domain.
        /// </summary>
        /// <param name="errorCode">
        /// The unique code that identifies the error within the domain.
        /// </param>
        /// <param name="errorDomain">
        /// The domain where the error has occurred.
        /// </param>
        /// <param name="message">
        /// A message that describes the error.
        /// </param>
        /// <param name="innerException">
        /// An exception that has caused the error.
        /// </param>
        public OperationException(string errorCode, string errorDomain, string? message, Exception? innerException)
            : base(message, innerException)
        {
            SetErrorDetails(errorCode, errorDomain);
        }

        /// <summary>
        /// The unique code that identifies the error within the domain.
        /// </summary>
        public string ErrorCode { get; private set; }

        string IOperationError.Code => ErrorCode;

        /// <summary>
        /// The domain where the error has occurred.
        /// </summary>
        public string ErrorDomain { get; private set; }

        string IOperationError.Domain => ErrorDomain;

        IOperationError? IOperationError.InnerError => InnerException as IOperationError;

        private void SetErrorDetails(string errorCode, string errorDomain)
        {
            ArgumentNullException.ThrowIfNull(errorCode, nameof(errorCode));
            ArgumentNullException.ThrowIfNull(errorDomain, nameof(errorDomain));

            ErrorCode = errorCode;
            ErrorDomain = errorDomain;

            Data[nameof(ErrorCode)] = errorCode;
            Data[nameof(ErrorDomain)] = errorDomain;
        }
    }
}
