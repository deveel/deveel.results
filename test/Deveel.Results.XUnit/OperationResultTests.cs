using Deveel;

using System.ComponentModel.DataAnnotations;

namespace Deveel;

public static class OperationResultTests
{
    [Fact]
    public static void OperationResult_Success()
    {
        var result = OperationResult.Success;
        Assert.Equal(OperationResultType.Success, result.ResultType);
        Assert.Null(result.Error);
    }

    [Fact]
    public static void OperationResult_NotChanged()
    {
        var result = OperationResult.NotChanged;
        Assert.Equal(OperationResultType.Unchanged, result.ResultType);
        Assert.Null(result.Error);
    }

    [Fact]
    public static void OperationResult_Cancelled()
    {
        var result = OperationResult.Cancelled;
        Assert.Equal(OperationResultType.Cancelled, result.ResultType);
        Assert.Null(result.Error);
    }

    [Fact]
    public static void OperationResult_Fail()
    {
        var error = new OperationError("err.1", "biz");
        var result = OperationResult.Fail(error);
        Assert.Equal(OperationResultType.Error, result.ResultType);
        Assert.NotNull(result.Error);
        Assert.Equal(error.Code, result.Error.Code);
        Assert.Equal(error.Domain, result.Error.Domain);
    }

    [Fact]
    public static void OperationResult_Fail_WithInner()
    {
        var inner = new OperationError("err.2", "biz");
        var error = new OperationError("err.1", "biz", innerError: inner);
        var result = OperationResult.Fail(error);
        Assert.Equal(OperationResultType.Error, result.ResultType);
        Assert.NotNull(result.Error);
        Assert.Equal(error.Code, result.Error.Code);
        Assert.Equal(error.Domain, result.Error.Domain);
        Assert.NotNull(result.Error.InnerError);
        Assert.Equal(inner.Code, result.Error.InnerError.Code);
        Assert.Equal(inner.Domain, result.Error.InnerError.Domain);
    }

    [Fact]
    public static void OperationResult_Fail_WithMessage()
    {
        var error = new OperationError("err.1", "biz", "An error occurred");
        var result = OperationResult.Fail(error);
        Assert.Equal(OperationResultType.Error, result.ResultType);
        Assert.NotNull(result.Error);
        Assert.Equal(error.Code, result.Error.Code);
        Assert.Equal(error.Domain, result.Error.Domain);
        Assert.Equal(error.Message, result.Error.Message);
    }

    [Fact]
    public static void OperationResult_Fail_WithMessageAndInner()
    {
        var inner = new OperationError("err.2", "biz");
        var error = new OperationError("err.1", "biz", "An error occurred", innerError: inner);
        var result = OperationResult.Fail(error);
        Assert.Equal(OperationResultType.Error, result.ResultType);
        Assert.NotNull(result.Error);
        Assert.Equal(error.Code, result.Error.Code);
        Assert.Equal(error.Domain, result.Error.Domain);
        Assert.Equal(error.Message, result.Error.Message);
        Assert.NotNull(result.Error.InnerError);
        Assert.Equal(inner.Code, result.Error.InnerError.Code);
        Assert.Equal(inner.Domain, result.Error.InnerError.Domain);
    }

    [Fact]
    public static void ImplicitConvertOperationError_Fail()
    {
        var error = new OperationError("err.1", "biz");
        OperationResult result = error;

        Assert.Equal(OperationResultType.Error, result.ResultType);
        Assert.NotNull(result.Error);
        Assert.Equal(error.Code, result.Error.Code);
        Assert.Equal(error.Domain, result.Error.Domain);
    }

    [Fact]
    public static void ImplicitConvertOperationException_Fail()
    {
        var exception = new OperationException("err.1", "biz");
        OperationResult result = exception;

        Assert.Equal(OperationResultType.Error, result.ResultType);
        Assert.NotNull(result.Error);
        Assert.Equal(exception.ErrorCode, result.Error.Code);
        Assert.Equal(exception.ErrorDomain, result.Error.Domain);
    }

    [Fact]
    public static void Match_Success()
    {
        var result = OperationResult.Success;
        var value = result.Match(() => 42);
        Assert.Equal(42, value);
    }

    [Fact]
    public static void Match_Error()
    {
        var error = new OperationError("err.1", "biz");
        var result = OperationResult.Fail(error);
        var value = result.Match(ifError: e => $"The error {e.Code} was generated in domain {e.Domain}");
        Assert.Equal("The error err.1 was generated in domain biz", value);
    }

    [Fact]
    public static void Match_Unchanged()
    {
        var result = OperationResult.NotChanged;
        var value = result.Match(() => 42, ifUnchanged: () => 0);
        Assert.Equal(0, value);
    }

    [Fact]
    public static async Task MatchAsync_Success()
    {
        var result = OperationResult.Success;
        var value = await result.MatchAsync(() => Task.FromResult(42));
        Assert.Equal(42, value);
    }

    [Fact]
    public static async Task MatchAsync_Error()
    {
        var error = new OperationError("err.1", "biz");
        var result = OperationResult.Fail(error);
        var value = await result.MatchAsync(ifError: e => Task.FromResult($"The error {e.Code} was generated in domain {e.Domain}"));
        Assert.Equal("The error err.1 was generated in domain biz", value);
    }

    [Fact]
    public static async Task MatchAsync_Unchanged()
    {
        var result = OperationResult.NotChanged;
        var value = await result.MatchAsync(() => Task.FromResult(42), ifUnchanged: () => Task.FromResult(0));
        Assert.Equal(0, value);
    }

    [Fact]
    public static void OperationResult_IsSuccess()
    {
        var result = OperationResult.Success;
        Assert.True(result.IsSuccess());
    }

    [Fact]
    public static void OperationResult_IsError()
    {
        var error = new OperationError("err.1", "biz");
        var result = OperationResult.Fail(error);
        Assert.True(result.IsError());
    }

    [Fact]
    public static void OperationResult_IsCancelled()
    {
        var result = OperationResult.Cancelled;
        Assert.True(result.IsCancelled());
    }

    [Fact]
    public static void OperationResult_IsUnchanged()
    {
        var result = OperationResult.NotChanged;
        Assert.True(result.IsUnchanged());
    }

    [Fact]
    public static void OperationResult_ValidationErrors()
    {
        var errors = new List<ValidationResult> {
            new ValidationResult("The value is required", new[] { "value" }),
            new ValidationResult("The value must be greater than 0", new[] { "value" })
        };

        var error = new OperationValidationError("err.1", "biz", errors);
        var result = OperationResult.Fail(error);

        Assert.True(result.HasValidationErrors());
        Assert.Equal(errors, result.ValidationResults());
    }

    [Fact]
    public static void OperationResult_Fail_WithValidationErrors()
    {
        var errors = new List<ValidationResult> {
            new ValidationResult("The value is required", new[] { "value" }),
            new ValidationResult("The value must be greater than 0", new[] { "value" })
        };

        var result = OperationResult.ValidationFailed("err.1", "biz", errors);

        Assert.True(result.IsError());
        Assert.True(result.HasValidationErrors());
        Assert.Equal(errors, result.ValidationResults());
    }
}
