namespace Deveel;

public static class ExceptionExtensionsTests
{
    #region Basic Conversion Tests

    [Fact]
    public static void AsOperationError_WithSimpleException_CreatesOperationError()
    {
        var exception = new InvalidOperationException("Test error message");

        var error = exception.AsOperationError();

        Assert.NotNull(error);
        Assert.IsType<OperationError>(error);
        Assert.Equal("ERROR", error.Code);
        Assert.Equal("Test error message", error.Message);
        Assert.Equal("System", error.Domain);
    }

    [Fact]
    public static void AsOperationError_WithCustomCode_UsesProvidedCode()
    {
        var exception = new ArgumentException("Invalid argument");

        var error = exception.AsOperationError("INVALID_ARG");

        Assert.NotNull(error);
        Assert.Equal("INVALID_ARG", error.Code);
        Assert.Equal("Invalid argument", error.Message);
    }

    [Fact]
    public static void AsOperationError_WithCustomDomain_UsesProvidedDomain()
    {
        var exception = new NullReferenceException("Object is null");

        var error = exception.AsOperationError("ERROR", "CustomDomain");

        Assert.NotNull(error);
        Assert.Equal("ERROR", error.Code);
        Assert.Equal("CustomDomain", error.Domain);
        Assert.Equal("Object is null", error.Message);
    }

    [Fact]
    public static void AsOperationError_WithCodeAndDomain_UsesBothParameters()
    {
        var exception = new FormatException("Invalid format");

        var error = exception.AsOperationError("FORMAT_ERROR", "Validation");

        Assert.NotNull(error);
        Assert.Equal("FORMAT_ERROR", error.Code);
        Assert.Equal("Validation", error.Domain);
        Assert.Equal("Invalid format", error.Message);
    }

    [Fact]
    public static void AsOperationError_WithEmptyCode_DefaultsToError()
    {
        var exception = new IOException("File not found");

        var error = exception.AsOperationError("");

        Assert.Equal("ERROR", error.Code);
    }

    [Fact]
    public static void AsOperationError_WithWhitespaceCode_DefaultsToError()
    {
        var exception = new TimeoutException("Operation timed out");

        var error = exception.AsOperationError("   ");

        Assert.Equal("ERROR", error.Code);
    }

    [Fact]
    public static void AsOperationError_WithNullCode_DefaultsToError()
    {
        var exception = new NotImplementedException("Not yet implemented");

        var error = exception.AsOperationError(null);

        Assert.Equal("ERROR", error.Code);
    }

    #endregion

    #region Domain Inference Tests

    [Fact]
    public static void AsOperationError_InfersNamespaceAsDomain()
    {
        var exception = new System.Collections.Generic.KeyNotFoundException("Key not found");

        var error = exception.AsOperationError();

        Assert.Equal("System.Collections.Generic", error.Domain);
    }

    [Fact]
    public static void AsOperationError_WithNullDomainParameter_InfersNamespace()
    {
        var exception = new System.IO.FileNotFoundException("File not found");

        var error = exception.AsOperationError("FILE_ERROR", null);

        Assert.Equal("System.IO", error.Domain);
    }

    [Fact]
    public static void AsOperationError_WithEmptyDomainParameter_UsesEmptyDomain()
    {
        var exception = new Exception("Test");

        var error = exception.AsOperationError("TEST", "");

        Assert.Equal("", error.Domain);
    }

    #endregion

    #region Inner Exception Tests

    [Fact]
    public static void AsOperationError_WithInnerException_ConvertsInnerException()
    {
        var innerException = new ArgumentException("Invalid argument");
        var outerException = new InvalidOperationException("Operation failed", innerException);

        var error = outerException.AsOperationError("OUTER");

        Assert.NotNull(error);
        Assert.Equal("OUTER", error.Code);
        Assert.Equal("Operation failed", error.Message);
        Assert.NotNull(error.InnerError);
        Assert.Equal("ERROR", error.InnerError.Code);
        Assert.Equal("Invalid argument", error.InnerError.Message);
    }

    [Fact]
    public static void AsOperationError_WithChainedInnerExceptions_ConvertsAllChain()
    {
        var innermost = new ArgumentException("Invalid argument");
        var middle = new InvalidOperationException("Operation failed", innermost);
        var outer = new SystemException("System error", middle);

        var error = outer.AsOperationError("OUTER_ERROR");

        Assert.NotNull(error);
        Assert.Equal("OUTER_ERROR", error.Code);
        
        Assert.NotNull(error.InnerError);
        Assert.Equal("ERROR", error.InnerError.Code);
        
        Assert.NotNull(error.InnerError.InnerError);
        Assert.Equal("ERROR", error.InnerError.InnerError.Code);
        
        Assert.Null(error.InnerError.InnerError.InnerError);
    }

    [Fact]
    public static void AsOperationError_WithInnerException_PreservesDomainAcrossChain()
    {
        var innerException = new ArgumentException("Inner error");
        var outerException = new InvalidOperationException("Outer error", innerException);

        var error = outerException.AsOperationError("OUTER", "CustomDomain");

        Assert.NotNull(error.InnerError);
        // Inner exception should inherit the domain from the domain parameter
        Assert.Equal("CustomDomain", error.InnerError.Domain);
    }

    [Fact]
    public static void AsOperationError_WithoutInnerException_InnerErrorIsNull()
    {
        var exception = new Exception("Standalone exception");

        var error = exception.AsOperationError();

        Assert.Null(error.InnerError);
    }

    #endregion

    #region OperationError Input Tests

    [Fact]
    public static void AsOperationError_WithOperationException_ReturnsOperationError()
    {
        var operationException = new OperationException("OP_ERROR", "OpDomain", "Operation error");

        var error = operationException.AsOperationError();

        Assert.NotNull(error);
        Assert.Equal("OP_ERROR", error.Code);
        Assert.Equal("OpDomain", error.Domain);
    }

    #endregion

    #region Message Handling Tests

    [Fact]
    public static void AsOperationError_WithNullMessage_CreatesError()
    {
        #pragma warning disable CS8625
        var exception = new Exception(null);
        #pragma warning restore CS8625

        var error = exception.AsOperationError();

        // Exception converts null message to empty string
        Assert.NotNull(error);
        Assert.Equal("ERROR", error.Code);
    }

    [Fact]
    public static void AsOperationError_WithEmptyMessage_PreservesEmpty()
    {
        var exception = new Exception("");

        var error = exception.AsOperationError();

        Assert.Equal("", error.Message);
    }

    [Fact]
    public static void AsOperationError_WithComplexMessage_PreservesMessage()
    {
        var complexMessage = "Complex error: Object reference not set to an instance of an object. Stack trace: ...";
        var exception = new Exception(complexMessage);

        var error = exception.AsOperationError();

        Assert.Equal(complexMessage, error.Message);
    }

    #endregion

    #region Different Exception Types Tests

    [Fact]
    public static void AsOperationError_WithArgumentNullException_Converts()
    {
        var exception = new ArgumentNullException("paramName", "Parameter cannot be null");

        var error = exception.AsOperationError("NULL_PARAM");

        Assert.NotNull(error);
        Assert.Equal("NULL_PARAM", error.Code);
        Assert.Equal("System", error.Domain);
    }

    [Fact]
    public static void AsOperationError_WithIndexOutOfRangeException_Converts()
    {
        var exception = new IndexOutOfRangeException("Index out of range");

        var error = exception.AsOperationError("INDEX_ERROR", "ArrayOps");

        Assert.Equal("INDEX_ERROR", error.Code);
        Assert.Equal("ArrayOps", error.Domain);
    }

    [Fact]
    public static void AsOperationError_WithDivideByZeroException_Converts()
    {
        var exception = new DivideByZeroException("Attempted to divide by zero");

        var error = exception.AsOperationError("MATH_ERROR");

        Assert.Equal("MATH_ERROR", error.Code);
        Assert.Equal("Attempted to divide by zero", error.Message);
    }

    [Fact]
    public static void AsOperationError_WithAggregateException_Converts()
    {
        var inner1 = new ArgumentException("Arg error");
        var inner2 = new Exception("Generic error");
        var aggregateException = new AggregateException("Multiple errors", inner1, inner2);

        var error = aggregateException.AsOperationError("AGGREGATE");

        Assert.Equal("AGGREGATE", error.Code);
        Assert.NotNull(error.InnerError);
    }

    [Fact]
    public static void AsOperationError_WithHttpRequestException_Converts()
    {
        var exception = new System.Net.Http.HttpRequestException("Request failed");

        var error = exception.AsOperationError("HTTP_ERROR");

        Assert.Equal("HTTP_ERROR", error.Code);
        Assert.Equal("System.Net.Http", error.Domain);
    }

    [Fact]
    public static void AsOperationError_WithTaskCanceledException_Converts()
    {
        var exception = new TaskCanceledException("Task was cancelled");

        var error = exception.AsOperationError("CANCELLED");

        Assert.Equal("CANCELLED", error.Code);
        Assert.Equal("Task was cancelled", error.Message);
    }

    #endregion

    #region Code Parameter Edge Cases

    [Fact]
    public static void AsOperationError_WithSingleCharCode_AcceptsCode()
    {
        var exception = new Exception("Error");

        var error = exception.AsOperationError("E");

        Assert.Equal("E", error.Code);
    }

    [Fact]
    public static void AsOperationError_WithVeryLongCode_AcceptsCode()
    {
        var exception = new Exception("Error");
        var longCode = new string('X', 1000);

        var error = exception.AsOperationError(longCode);

        Assert.Equal(longCode, error.Code);
    }

    [Fact]
    public static void AsOperationError_WithSpecialCharactersInCode_Preserves()
    {
        var exception = new Exception("Error");
        var specialCode = "ERROR_CODE-123!@#";

        var error = exception.AsOperationError(specialCode);

        Assert.Equal(specialCode, error.Code);
    }

    #endregion

    #region Domain Parameter Edge Cases

    [Fact]
    public static void AsOperationError_WithDomainParameter_OverridesNamespace()
    {
        var exception = new System.IO.IOException("IO error");

        var error = exception.AsOperationError("ERROR", "CustomDomain");

        Assert.Equal("CustomDomain", error.Domain);
    }

    [Fact]
    public static void AsOperationError_WithWhitespaceDomain_UsesWhitespace()
    {
        var exception = new Exception("Error");

        var error = exception.AsOperationError("ERROR", "   ");

        Assert.Equal("   ", error.Domain);
    }

    #endregion

    #region Chaining and Composition Tests

    [Fact]
    public static void AsOperationError_ResultCanBeUsedInOperationResult()
    {
        var exception = new InvalidOperationException("Invalid operation");

        var error = exception.AsOperationError("OP_FAILED");
        var result = OperationResult.Fail(error);

        Assert.True(result.IsError());
        Assert.Equal("OP_FAILED", result.Error?.Code);
        Assert.Equal("Invalid operation", result.Error?.Message);
    }

    [Fact]
    public static void AsOperationError_ResultCanBeChainedWithGenericResult()
    {
        var exception = new ArgumentException("Missing required argument");

        var error = exception.AsOperationError("ARG_MISSING", "Arguments");
        var result = OperationResult<int>.Fail(error);

        Assert.True(result.IsError());
        Assert.Equal("ARG_MISSING", result.Error?.Code);
    }

    [Fact]
    public static void AsOperationError_CanBeUsedInBindChain()
    {
        var divideByZeroEx = new DivideByZeroException("Cannot divide by zero");

        var result = OperationResult.Success
            .Bind(() =>
            {
                throw divideByZeroEx;
            });

        Assert.True(result.IsError());
        Assert.Equal("UnhandledException", result.Error?.Code);
    }

    #endregion

    #region Recursive Inner Exception Tests

    [Fact]
    public static void AsOperationError_WithMultipleLevelsOfInnerExceptions_ConvertsAll()
    {
        var level3 = new Exception("Level 3 error");
        var level2 = new Exception("Level 2 error", level3);
        var level1 = new Exception("Level 1 error", level2);

        var error = level1.AsOperationError("LEVEL1");

        var current = error;
        int level = 1;
        while (current != null && level <= 3)
        {
            Assert.NotNull(current);
            level++;
            current = current.InnerError;
        }
        Assert.Null(current);
    }

    [Fact]
    public static void AsOperationError_InnerExceptionsPreserveOrder()
    {
        var ex3 = new Exception("Third");
        var ex2 = new Exception("Second", ex3);
        var ex1 = new Exception("First", ex2);

        var error = ex1.AsOperationError("TOP");

        Assert.Equal("First", error.Message);
        Assert.NotNull(error.InnerError);
        Assert.Equal("Second", error.InnerError.Message);
        Assert.NotNull(error.InnerError.InnerError);
        Assert.Equal("Third", error.InnerError.InnerError.Message);
    }

    #endregion

    #region Real-World Scenario Tests

    [Fact]
    public static void AsOperationError_FileIOException_Scenario()
    {
        var exception = new System.IO.FileNotFoundException("File not found: config.json");

        var error = exception.AsOperationError("FILE_NOT_FOUND", "FileSystem");

        Assert.Equal("FILE_NOT_FOUND", error.Code);
        Assert.Equal("FileSystem", error.Domain);
        Assert.Contains("config.json", error.Message);
    }

    [Fact]
    public static void AsOperationError_DatabaseException_Scenario()
    {
        var innerException = new Exception("Timeout expired");
        var dbException = new Exception("Database connection failed", innerException);

        var error = dbException.AsOperationError("DB_ERROR", "Database");

        Assert.Equal("DB_ERROR", error.Code);
        Assert.Equal("Database", error.Domain);
        Assert.NotNull(error.InnerError);
        Assert.Equal("Timeout expired", error.InnerError.Message);
    }

    [Fact]
    public static void AsOperationError_ValidationException_Scenario()
    {
        var ex = new ArgumentException("Password must be at least 8 characters");

        var error = ex.AsOperationError("VALIDATION_FAILED", "Security");

        Assert.Equal("VALIDATION_FAILED", error.Code);
        Assert.Equal("Security", error.Domain);
        Assert.Contains("Password", error.Message);
    }

    [Fact]
    public static void AsOperationError_NetworkException_Scenario()
    {
        var innerException = new TimeoutException("Request timeout");
        var networkException = new System.Net.Http.HttpRequestException("Failed to connect to server", innerException);

        var error = networkException.AsOperationError("NETWORK_ERROR", "Connectivity");

        Assert.Equal("NETWORK_ERROR", error.Code);
        Assert.Equal("Connectivity", error.Domain);
        Assert.Contains("Failed to connect", error.Message);
        Assert.NotNull(error.InnerError);
    }

    #endregion

    #region Null Safety Tests

    [Fact]
    public static void AsOperationError_WithNullInnerException_HandlesGracefully()
    {
        var exception = new Exception("Test", null);

        var error = exception.AsOperationError();

        Assert.Null(error.InnerError);
    }

    #endregion
}








