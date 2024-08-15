using PostSharp.Aspects;
using PostSharp.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Reflection;

namespace WebAPI.Aspects
{
    [PSerializable]
    public class ExceptionAspect : OnExceptionAspect
    {
        private readonly ILogger<ExceptionAspect> _logger;

        public ExceptionAspect(ILogger<ExceptionAspect> logger)
        {
            _logger = logger;
        }

        public override void OnException(MethodExecutionArgs args)
        {
            var exception = args.Exception;
            var method = args.Method as MethodInfo;
            var methodName = method.Name;
            var className = method.DeclaringType.FullName;

            // Log the exception with detailed information
            _logger.LogError(exception, 
                "An exception occurred in {ClassName}.{MethodName}. " +
                "Exception Type: {ExceptionType}. " +
                "Message: {ExceptionMessage}. " +
                "Stack Trace: {StackTrace}", 
                className, methodName, exception.GetType().Name, exception.Message, exception.StackTrace);

            // Create an appropriate error response
            var errorResponse = CreateErrorResponse(exception);

            // Handle the return value based on the method's return type
            if (typeof(Task).IsAssignableFrom(method.ReturnType))
            {
                if (method.ReturnType.IsGenericType)
                {
                    // For Task<T>
                    var resultType = method.ReturnType.GetGenericArguments()[0];
                    if (typeof(IActionResult).IsAssignableFrom(resultType))
                    {
                        args.ReturnValue = Task.FromResult(errorResponse);
                    }
                    else
                    {
                        // If it's not IActionResult, wrap it in a Task<ObjectResult>
                        args.ReturnValue = Task.FromResult<object>(new ObjectResult(errorResponse) { StatusCode = (int)HttpStatusCode.InternalServerError });
                    }
                }
                else
                {
                    // For Task (non-generic)
                    args.ReturnValue = Task.CompletedTask;
                }
            }
            else if (typeof(IActionResult).IsAssignableFrom(method.ReturnType))
            {
                // For synchronous methods returning IActionResult
                args.ReturnValue = errorResponse;
            }
            else
            {
                // For other return types, wrap in ObjectResult
                args.ReturnValue = new ObjectResult(errorResponse) { StatusCode = (int)HttpStatusCode.InternalServerError };
            }

            args.FlowBehavior = FlowBehavior.Return;
        }

        private IActionResult CreateErrorResponse(Exception exception)
        {
            var statusCode = DetermineStatusCode(exception);
            var errorMessage = GetUserFriendlyErrorMessage(exception);

            var problemDetails = new ProblemDetails
            {
                Status = (int)statusCode,
                Title = "An error occurred",
                Detail = errorMessage,
                Instance = Guid.NewGuid().ToString()
            };

            // Add custom properties for debugging (only in development environment)
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                problemDetails.Extensions["exception"] = new
                {
                    exception.Message,
                    exception.StackTrace,
                    exception.GetType().Name
                };
            }

            return new ObjectResult(problemDetails)
            {
                StatusCode = (int)statusCode
            };
        }

        private HttpStatusCode DetermineStatusCode(Exception exception)
        {
            return exception switch
            {
                UnauthorizedAccessException _ => HttpStatusCode.Unauthorized,
                ArgumentException _ => HttpStatusCode.BadRequest,
                KeyNotFoundException _ => HttpStatusCode.NotFound,
                NotImplementedException _ => HttpStatusCode.NotImplemented,
                TimeoutException _ => HttpStatusCode.RequestTimeout,
                // Add more custom exceptions here
                _ => HttpStatusCode.InternalServerError
            };
        }

        private string GetUserFriendlyErrorMessage(Exception exception)
        {
            return exception switch
            {
                UnauthorizedAccessException _ => "You are not authorized to perform this action.",
                ArgumentException _ => "Invalid input provided.",
                KeyNotFoundException _ => "The requested resource was not found.",
                NotImplementedException _ => "This feature is not yet implemented.",
                TimeoutException _ => "The operation timed out. Please try again later.",
                // Add more custom exceptions here
                _ => "An unexpected error occurred. Please try again later."
            };
        }
    }
}