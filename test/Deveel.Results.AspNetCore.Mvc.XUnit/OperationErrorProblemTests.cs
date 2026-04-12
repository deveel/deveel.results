using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace Deveel;

public static class OperationErrorProblemTests
{
    [Fact]
    public static void OperationResultError_AsProblemDetailsFromFactory_ProblemDetails()
    {
        var services = new ServiceCollection();
        services.AddProblemDetails();
        services.AddOptions<ApiBehaviorOptions>();
        services.AddSingleton<ProblemDetailsFactory, DefaultProblemDetailsFactory>();
        
        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };
        
        var errorResult = OperationResult.Fail("ERR-01", "App", "The operation failed");
        
        var problemResult = errorResult.AsProblem(httpContext);
        
        Assert.NotNull(problemResult);
        var objResult = Assert.IsType<ProblemHttpResult>(problemResult);
        
        Assert.Equal(400, objResult.ProblemDetails.Status);
        Assert.Equal("The operation failed", objResult.ProblemDetails.Detail);
        Assert.Equal("ERR-01", objResult.ProblemDetails.Extensions["errorCode"]);
        Assert.Equal("App", objResult.ProblemDetails.Extensions["domain"]);
    }

    [Fact]
    public static void AsProblem_NullResult_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();
        services.AddProblemDetails();
        services.AddOptions<ApiBehaviorOptions>();
        services.AddSingleton<ProblemDetailsFactory, DefaultProblemDetailsFactory>();
        
        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };
        
        Assert.Throws<ArgumentNullException>(() => ((IOperationResult)null!).AsProblem(httpContext));
    }

    [Fact]
    public static void AsProblem_NonErrorResult_ThrowsArgumentException()
    {
        var services = new ServiceCollection();
        services.AddProblemDetails();
        services.AddOptions<ApiBehaviorOptions>();
        services.AddSingleton<ProblemDetailsFactory, DefaultProblemDetailsFactory>();
        
        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };
        
        var successResult = OperationResult.Success;
        
        Assert.Throws<ArgumentException>(() => successResult.AsProblem(httpContext));
    }

    [Fact]
    public static void OperationResultValidationError_AsProblemDetailsFromFactory_ValidationProblemDetails()
    {
        var services = new ServiceCollection();
        services.AddProblemDetails();
        services.AddOptions<ApiBehaviorOptions>();
        services.AddSingleton<ProblemDetailsFactory, DefaultProblemDetailsFactory>();
        
        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };
        
        var validationResults = new List<ValidationResult>
        {
            new ValidationResult("Name is required", new[] { "Name" }),
            new ValidationResult("Email is invalid", new[] { "Email" }),
            new ValidationResult("ErrorMessage", new[] { "SomeMember" })
        };
        var errorResult = OperationResult.ValidationFailed("VAL-01", "App", validationResults);
        
        var problemResult = errorResult.AsProblem(httpContext);
        
        Assert.NotNull(problemResult);
        var objResult = Assert.IsType<ProblemHttpResult>(problemResult);
        
        Assert.Equal(400, objResult.ProblemDetails.Status);
        Assert.Null(objResult.ProblemDetails.Detail);
        Assert.Equal("VAL-01", objResult.ProblemDetails.Extensions["errorCode"]);
        Assert.Equal("App", objResult.ProblemDetails.Extensions["domain"]);
        
        // For validation errors, it should be ValidationProblemDetails
        var validationProblem = Assert.IsType<ValidationProblemDetails>(objResult.ProblemDetails);
        Assert.Contains("Name", validationProblem.Errors);
        Assert.Contains("Email", validationProblem.Errors);
        Assert.Contains("SomeMember", validationProblem.Errors);
    }

    [Fact]
    public static void OperationResultError_AsProblemDetailsWithoutFactory_ProblemDetails()
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().BuildServiceProvider()
        };
        
        var errorResult = OperationResult.Fail("ERR-01", "App", "The operation failed");
        
        var problemResult = errorResult.AsProblem(httpContext);
        
        Assert.NotNull(problemResult);
        var objResult = Assert.IsType<ProblemHttpResult>(problemResult);
        
        Assert.Equal(400, objResult.ProblemDetails.Status);
        Assert.Equal("Bad Request", objResult.ProblemDetails.Title);
        Assert.Equal("The operation failed", objResult.ProblemDetails.Detail);
        Assert.Equal("ERR-01", objResult.ProblemDetails.Extensions["errorCode"]);
        Assert.Equal("App", objResult.ProblemDetails.Extensions["domain"]);
    }

    [Fact]
    public static void OperationResultValidationError_AsProblemDetailsWithoutFactory_ValidationProblemDetails()
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().BuildServiceProvider()
        };
        
        var validationResults = new List<ValidationResult>
        {
            new ValidationResult("Name is required", new[] { "Name" }),
            new ValidationResult("Email is invalid", new[] { "Email" })
        };
        var errorResult = OperationResult.ValidationFailed("VAL-01", "App", validationResults);
        
        var problemResult = errorResult.AsProblem(httpContext);
        
        Assert.NotNull(problemResult);
        var objResult = Assert.IsType<ProblemHttpResult>(problemResult);
        
        Assert.Equal(400, objResult.ProblemDetails.Status);
        Assert.Equal("Bad Request", objResult.ProblemDetails.Title);
        Assert.Null(objResult.ProblemDetails.Detail);
        Assert.Equal("VAL-01", objResult.ProblemDetails.Extensions["errorCode"]);
        Assert.Equal("App", objResult.ProblemDetails.Extensions["domain"]);
        
        // For validation errors, it should be ValidationProblemDetails
        var validationProblem = Assert.IsType<ValidationProblemDetails>(objResult.ProblemDetails);
        Assert.Contains("Name", validationProblem.Errors);
        Assert.Contains("Email", validationProblem.Errors);
    }

    [Fact]
    public static void AsProblem_WithCustomStatusCode_ReturnsCustomStatusCode()
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().BuildServiceProvider()
        };
        
        var errorResult = OperationResult.Fail("ERR-01", "App", "The operation failed");
        
        var problemResult = errorResult.AsProblem(httpContext, statusCode: 500);
        
        Assert.NotNull(problemResult);
        var objResult = Assert.IsType<ProblemHttpResult>(problemResult);
        
        Assert.Equal(500, objResult.ProblemDetails.Status);
        Assert.Equal("The operation failed", objResult.ProblemDetails.Detail);
        Assert.Equal("ERR-01", objResult.ProblemDetails.Extensions["errorCode"]);
    }

    [Fact]
    public static void AsProblem_WithCustomTitle_ReturnsCustomTitle()
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().BuildServiceProvider()
        };
        
        var errorResult = OperationResult.Fail("ERR-01", "App", "The operation failed");
        
        var problemResult = errorResult.AsProblem(httpContext, title: "Custom Error Title");
        
        Assert.NotNull(problemResult);
        var objResult = Assert.IsType<ProblemHttpResult>(problemResult);
        
        Assert.Equal("Custom Error Title", objResult.ProblemDetails.Title);
    }

    [Fact]
    public static void AsProblem_WithNullContext_ReturnsValidProblem()
    {
        var errorResult = OperationResult.Fail("ERR-01", "App", "The operation failed");
        
        var problemResult = errorResult.AsProblem(context: null);
        
        Assert.NotNull(problemResult);
        var objResult = Assert.IsType<ProblemHttpResult>(problemResult);
        
        Assert.Equal(400, objResult.ProblemDetails.Status);
        Assert.Equal("The operation failed", objResult.ProblemDetails.Detail);
        Assert.Equal("ERR-01", objResult.ProblemDetails.Extensions["errorCode"]);
        Assert.Equal("App", objResult.ProblemDetails.Extensions["domain"]);
    }

    [Fact]
    public static void AsProblem_ErrorWithoutMessage_ReturnsNullDetail()
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().BuildServiceProvider()
        };
        
        var errorResult = OperationResult.Fail("ERR-01", "App", message: null);
        
        var problemResult = errorResult.AsProblem(httpContext);
        
        Assert.NotNull(problemResult);
        var objResult = Assert.IsType<ProblemHttpResult>(problemResult);
        
        Assert.Null(objResult.ProblemDetails.Detail);
        Assert.Equal("ERR-01", objResult.ProblemDetails.Extensions["errorCode"]);
    }

    [Fact]
    public static void OperationResultOfT_Error_AsProblem_ReturnsProblem()
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().BuildServiceProvider()
        };
        
        var errorResult = OperationResult<int>.Fail("ERR-01", "App", "Operation failed");
        
        var problemResult = ((IOperationResult)errorResult).AsProblem(httpContext);
        
        Assert.NotNull(problemResult);
        var objResult = Assert.IsType<ProblemHttpResult>(problemResult);
        
        Assert.Equal(400, objResult.ProblemDetails.Status);
        Assert.Equal("Operation failed", objResult.ProblemDetails.Detail);
        Assert.Equal("ERR-01", objResult.ProblemDetails.Extensions["errorCode"]);
        Assert.Equal("App", objResult.ProblemDetails.Extensions["domain"]);
    }

    [Fact]
    public static void OperationResultOfT_ValidationError_AsProblem_ReturnsValidationProblem()
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().BuildServiceProvider()
        };
        
        var validationResults = new List<ValidationResult>
        {
            new ValidationResult("Age must be positive", new[] { "Age" })
        };
        var errorResult = OperationResult<int>.ValidationFailed("VAL-01", "App", validationResults);
        
        var problemResult = ((IOperationResult)errorResult).AsProblem(httpContext);
        
        Assert.NotNull(problemResult);
        var objResult = Assert.IsType<ProblemHttpResult>(problemResult);
        
        Assert.IsType<ValidationProblemDetails>(objResult.ProblemDetails);
        Assert.Equal("VAL-01", objResult.ProblemDetails.Extensions["errorCode"]);
    }

    [Fact]
    public static void AsProblem_WithCustomStatusCodeAndTitle_ReturnsCustomValues()
    {
        var services = new ServiceCollection();
        services.AddProblemDetails();
        services.AddOptions<ApiBehaviorOptions>();
        services.AddSingleton<ProblemDetailsFactory, DefaultProblemDetailsFactory>();
        
        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };
        
        var errorResult = OperationResult.Fail("ERR-01", "App", "The operation failed");
        
        var problemResult = errorResult.AsProblem(httpContext, statusCode: 422, title: "Unprocessable Entity");
        
        Assert.NotNull(problemResult);
        var objResult = Assert.IsType<ProblemHttpResult>(problemResult);
        
        Assert.Equal(422, objResult.ProblemDetails.Status);
        Assert.Equal("Unprocessable Entity", objResult.ProblemDetails.Title);
        Assert.Equal("The operation failed", objResult.ProblemDetails.Detail);
    }

    [Fact]
    public static void AsProblem_ValidationErrorWithCustomStatusCode_ReturnsValidationProblemWithCustomStatus()
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().BuildServiceProvider()
        };
        
        var validationResults = new List<ValidationResult>
        {
            new ValidationResult("Name is required", new[] { "Name" })
        };
        var errorResult = OperationResult.ValidationFailed("VAL-01", "App", validationResults);
        
        var problemResult = errorResult.AsProblem(httpContext, statusCode: 422);
        
        Assert.NotNull(problemResult);
        var objResult = Assert.IsType<ProblemHttpResult>(problemResult);
        
        Assert.Equal(422, objResult.ProblemDetails.Status);
        var validationProblem = Assert.IsType<ValidationProblemDetails>(objResult.ProblemDetails);
        Assert.Contains("Name", validationProblem.Errors);
    }

    [Fact]
    public static void AsProblem_ValidationErrorWithMultipleMembersPerError_IncludesAllErrors()
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().BuildServiceProvider()
        };
        
        var validationResults = new List<ValidationResult>
        {
            new ValidationResult("This field is required", new[] { "Name", "Email" })
        };
        var errorResult = OperationResult.ValidationFailed("VAL-01", "App", validationResults);
        
        var problemResult = errorResult.AsProblem(httpContext);
        
        Assert.NotNull(problemResult);
        var objResult = Assert.IsType<ProblemHttpResult>(problemResult);
        
        var validationProblem = Assert.IsType<ValidationProblemDetails>(objResult.ProblemDetails);
        Assert.True(validationProblem.Errors.ContainsKey("Name"));
        Assert.True(validationProblem.Errors.ContainsKey("Email"));
    }

    [Fact]
    public static void AsProblem_ErrorWithNestedError_IncludesNestedErrorCode()
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().BuildServiceProvider()
        };
        
        var innerError = new OperationError("INNER-ERR", "Inner", "Inner error message");
        var errorResult = OperationResult.Fail("ERR-01", "App", "Outer error", innerError);
        
        var problemResult = errorResult.AsProblem(httpContext);
        
        Assert.NotNull(problemResult);
        var objResult = Assert.IsType<ProblemHttpResult>(problemResult);
        
        Assert.Equal("ERR-01", objResult.ProblemDetails.Extensions["errorCode"]);
        Assert.Equal("App", objResult.ProblemDetails.Extensions["domain"]);
    }

    [Fact]
    public static void AsProblem_ValidationErrorWithoutTitle_ReturnsDefaultTitle()
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().BuildServiceProvider()
        };
        
        var validationResults = new List<ValidationResult>
        {
            new ValidationResult("Name is required", new[] { "Name" })
        };
        var errorResult = OperationResult.ValidationFailed("VAL-01", "App", validationResults);
        
        var problemResult = errorResult.AsProblem(httpContext, title: null);
        
        Assert.NotNull(problemResult);
        var objResult = Assert.IsType<ProblemHttpResult>(problemResult);
        
        Assert.Equal("Bad Request", objResult.ProblemDetails.Title);
    }
}