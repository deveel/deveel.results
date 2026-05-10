namespace Deveel;

/// <summary>
/// Provides extension methods for <see cref="Exception"/> to facilitate
/// integration with the result-oriented pattern.
/// </summary>
public static class ExceptionExtensions
{
    /// <summary>
    /// Converts an <see cref="Exception"/> to an <see cref="IOperationError"/>,
    /// enabling exceptions to be used in result-oriented pipelines.
    /// </summary>
    /// <param name="exception">
    /// The exception to convert. If the exception already implements
    /// <see cref="IOperationError"/>, it is returned as-is.
    /// </param>
    /// <param name="code">
    /// An optional error code to assign to the resulting error.
    /// Defaults to <c>"ERROR"</c> when not specified or when an empty value is provided.
    /// </param>
    /// <param name="domain">
    /// An optional domain (namespace) to associate with the error.
    /// When not specified, the namespace of the exception type is used.
    /// </param>
    /// <returns>
    /// Returns an <see cref="IOperationError"/> that represents the exception.
    /// If the exception already implements <see cref="IOperationError"/>, the
    /// original instance is returned unchanged; otherwise a new <see cref="OperationError"/>
    /// is created using the exception message and, when present, the inner exception
    /// is recursively converted and set as the cause of the error.
    /// </returns>
    public static IOperationError AsOperationError(this Exception exception, string code = "ERROR", string? domain = null) {
        if (exception is IOperationError operationError)
            return operationError;
        
        if (string.IsNullOrWhiteSpace(code))
            code = "ERROR";
        
        domain ??= exception.GetType().Namespace!;
        return new OperationError(code, domain, exception.Message, exception.InnerException?.AsOperationError(domain: domain));
    }
}