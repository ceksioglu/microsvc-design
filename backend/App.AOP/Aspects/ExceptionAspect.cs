using System.Net;
using System.Reflection;
using Core.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PostSharp.Aspects;
using PostSharp.Serialization;

namespace AOP.Aspects
{
    [PSerializable]
    public class ExceptionAspect : OnExceptionAspect
    {
        public override void OnException(MethodExecutionArgs args)
        {
            // Resolve the logger at runtime
            var logger = args.Instance.GetType()
                .GetField("_loggerFactory", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(args.Instance) as ILoggerFactory;

            var exceptionLogger = logger?.CreateLogger<ExceptionAspect>();

            var exception = args.Exception;
            var method = args.Method as MethodInfo;
            var methodName = method.Name;
            var className = method.DeclaringType.FullName;

            // Log the exception with detailed information
            exceptionLogger?.LogError(exception, 
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
                        args.ReturnValue = Task.FromResult<object>(new ObjectResult(errorResponse) { StatusCode = (int)DetermineStatusCode(exception) });
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
                args.ReturnValue = new ObjectResult(errorResponse) { StatusCode = (int)DetermineStatusCode(exception) };
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
                NotFoundException _ => HttpStatusCode.NotFound,
                BadRequestException _ => HttpStatusCode.BadRequest,
                UnauthorizedException _ => HttpStatusCode.Unauthorized,
                ForbiddenException _ => HttpStatusCode.Forbidden,
                ConflictException _ => HttpStatusCode.Conflict,
                ArgumentException _ => HttpStatusCode.BadRequest,
                InvalidOperationException _ => HttpStatusCode.BadRequest,
                NotImplementedException _ => HttpStatusCode.NotImplemented,
                TimeoutException _ => HttpStatusCode.RequestTimeout,
                _ => HttpStatusCode.InternalServerError
            };
        }

        private string GetUserFriendlyErrorMessage(Exception exception)
        {
            return exception switch
            {
                NotFoundException _ => exception.Message,
                BadRequestException _ => exception.Message,
                UnauthorizedException _ => "You are not authorized to perform this action.",
                ForbiddenException _ => "You do not have permission to perform this action.",
                ConflictException _ => exception.Message,
                ArgumentException _ => "Invalid input provided.",
                InvalidOperationException _ => "The requested operation is invalid.",
                NotImplementedException _ => "This feature is not yet implemented.",
                TimeoutException _ => "The operation timed out. Please try again later.",
                _ => "An unexpected error occurred. Please try again later."
            };
        }
    }
}