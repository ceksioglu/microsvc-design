using Microsoft.Extensions.Logging;
using PostSharp.Aspects;
using PostSharp.Serialization;

namespace AOP.Aspects
{
    [PSerializable]
    public class LoggingAspect : OnMethodBoundaryAspect
    {
        public override void OnEntry(MethodExecutionArgs args)
        {
            var logger = ResolveLogger(args);
            var methodName = $"{args.Method.DeclaringType.FullName}.{args.Method.Name}";
            var parameters = string.Join(", ", args.Method.GetParameters().Select(p => $"{p.Name}: {args.Arguments[p.Position]}"));

            logger.LogInformation($"Entering method: {methodName}");
            logger.LogDebug($"Method parameters: {parameters}");
        }

        public override void OnSuccess(MethodExecutionArgs args)
        {
            var logger = ResolveLogger(args);
            var methodName = $"{args.Method.DeclaringType.FullName}.{args.Method.Name}";

            logger.LogInformation($"Successfully executed method: {methodName}");
            if (args.ReturnValue != null)
            {
                var returnValue = args.ReturnValue is Task task
                    ? GetTaskResult(task).Result
                    : args.ReturnValue;

                logger.LogDebug($"Method return value: {returnValue}");
            }
        }

        public override void OnException(MethodExecutionArgs args)
        {
            var logger = ResolveLogger(args);
            var methodName = $"{args.Method.DeclaringType.FullName}.{args.Method.Name}";

            logger.LogError(args.Exception, $"Exception in method: {methodName}");
        }

        private ILogger ResolveLogger(MethodExecutionArgs args)
        {
            var loggerFactory = args.Instance.GetType()
                .GetField("_loggerFactory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(args.Instance) as ILoggerFactory;

            if (loggerFactory == null)
            {
                throw new InvalidOperationException("Logger factory not found.");
            }

            return loggerFactory.CreateLogger(args.Method.DeclaringType);
        }

        private async Task<object> GetTaskResult(Task task)
        {
            await task;
            var resultProperty = task.GetType().GetProperty("Result");
            return resultProperty?.GetValue(task);
        }
    }
}