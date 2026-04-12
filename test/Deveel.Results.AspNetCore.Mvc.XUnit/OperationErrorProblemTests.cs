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
}