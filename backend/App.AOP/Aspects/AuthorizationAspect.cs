using System.Reflection;
using Core.Auth.abstracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PostSharp.Aspects;
using PostSharp.Serialization;

namespace AOP.Aspects
{
    [PSerializable]
    public class AuthorizationAspect : OnMethodBoundaryAspect
    {
        private string[] _requiredRoles;

        public AuthorizationAspect(params string[] requiredRoles)
        {
            _requiredRoles = requiredRoles;
        }

        public override void OnEntry(MethodExecutionArgs args)
        {
            var httpContext = ResolveHttpContext(args);
            var jwtService = ResolveJwtService(args);

            if (httpContext == null || jwtService == null)
            {
                throw new InvalidOperationException("Required services are not available.");
            }

            var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                SetUnauthorizedResult(args);
                return;
            }

            var token = authHeader.Substring("Bearer ".Length);
            var (isValid, principal) = jwtService.ValidateToken(token);

            if (!isValid || principal == null)
            {
                SetUnauthorizedResult(args);
                return;
            }

            if (_requiredRoles.Any() && !_requiredRoles.Any(role => principal.IsInRole(role)))
            {
                SetForbidResult(args);
                return;
            }

            httpContext.User = principal;
        }

        private HttpContext ResolveHttpContext(MethodExecutionArgs args)
        {
            var httpContextAccessor = args.Instance.GetType()
                .GetProperty("HttpContext", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(args.Instance) as IHttpContextAccessor;

            return httpContextAccessor?.HttpContext;
        }

        private IJwtService ResolveJwtService(MethodExecutionArgs args)
        {
            return args.Instance.GetType()
                .GetField("_jwtService", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(args.Instance) as IJwtService;
        }

        private void SetUnauthorizedResult(MethodExecutionArgs args)
        {
            SetResult(args, new UnauthorizedResult());
        }

        private void SetForbidResult(MethodExecutionArgs args)
        {
            SetResult(args, new ForbidResult());
        }

        private void SetResult(MethodExecutionArgs args, IActionResult result)
        {
            var methodInfo = args.Method as MethodInfo;
            if (methodInfo == null)
            {
                throw new InvalidOperationException("Method information is not available.");
            }

            if (typeof(Task).IsAssignableFrom(methodInfo.ReturnType))
            {
                if (methodInfo.ReturnType.IsGenericType)
                {
                    var genericType = methodInfo.ReturnType.GetGenericArguments()[0];
                    if (typeof(IActionResult).IsAssignableFrom(genericType))
                    {
                        args.ReturnValue = Task.FromResult(result);
                    }
                    else
                    {
                        args.ReturnValue = Task.FromResult<object>(new ObjectResult(result) { StatusCode = GetStatusCode(result) });
                    }
                }
                else
                {
                    args.ReturnValue = Task.CompletedTask;
                }
            }
            else if (typeof(IActionResult).IsAssignableFrom(methodInfo.ReturnType))
            {
                args.ReturnValue = result;
            }
            else
            {
                args.ReturnValue = new ObjectResult(result) { StatusCode = GetStatusCode(result) };
            }
            
            args.FlowBehavior = FlowBehavior.Return;
        }

        private int GetStatusCode(IActionResult result)
        {
            return result switch
            {
                UnauthorizedResult _ => StatusCodes.Status401Unauthorized,
                ForbidResult _ => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status400BadRequest
            };
        }
    }
}