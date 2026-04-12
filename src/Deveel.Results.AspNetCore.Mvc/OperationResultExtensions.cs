using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Deveel;

/// <summary>
/// Provides extension methods for converting <see cref="IOperationResult"/> instances
/// to ASP.NET Core problem details responses, allowing for standardized error handling in web applications.
/// </summary>
public static class OperationResultExtensions
{
    /// <summary>
    /// Converts an error operation result to an ASP.NET Core problem result.
    /// </summary>
    /// <param name="result">
    /// The operation result to convert to a problem result.
    /// </param>
    /// <param name="context">
    /// The HTTP context to use for creating the problem details.
    /// </param>
    /// <param name="statusCode">
    /// The HTTP status code to use in the problem details. Default is 400 Bad Request.
    /// </param>
    /// <param name="title">
    /// An optional title for the problem details.
    /// </param>
    /// <returns>
    /// Returns an <see cref="IResult"/> that represents the problem details response.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the <paramref name="result"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the <paramref name="result"/> is not an error result.
    /// </exception>
    public static IResult AsProblem(this IOperationResult result, HttpContext? context = null,
        int statusCode = StatusCodes.Status400BadRequest, 
        string? title = null)
    {
        if (result == null)
            throw new ArgumentNullException(nameof(result));
        if (!result.IsError())
            throw new  ArgumentException("Result must be an error result.", nameof(result));

        ModelStateDictionary? modelState = null;

        if (result.Error is IValidationError validationError)
        {
            modelState = new ModelStateDictionary();
            var memberErrors = validationError.GetMemberErrors();
            foreach (var memberError in memberErrors)
            foreach (var errorMessage in memberError.Value)
                modelState.AddModelError(memberError.Key, errorMessage);
        }
        
        ProblemDetails problemDetails;
        if (context?.RequestServices.GetService(typeof(ProblemDetailsFactory)) is ProblemDetailsFactory factory)
        {
            var message = result.Error?.Message;
            if (result.Error is IValidationError)
            {
                problemDetails = factory.CreateValidationProblemDetails(context, modelState!, statusCode, title, detail: message); 
            }
            else
            {
                problemDetails = factory.CreateProblemDetails(context, statusCode, title, detail: message);
            }
        }
        else
        {
            if (result.Error is IValidationError)
            {
                problemDetails = new ValidationProblemDetails(modelState!)
                {
                    Status = statusCode,
                    Title = title,
                    Detail = result.Error?.Message
                };
            }
            else
            {
                problemDetails = new ProblemDetails
                {
                    Status = statusCode,
                    Title = title,
                    Detail = result.Error?.Message
                };
            }
        }
        
        problemDetails.Extensions["errorCode"] = result.Error?.Code;
        problemDetails.Extensions["domain"] = result.Error?.Domain;

        return Results.Problem(problemDetails);
    }    
}