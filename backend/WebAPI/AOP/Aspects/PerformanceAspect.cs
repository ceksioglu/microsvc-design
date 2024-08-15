using PostSharp.Aspects;
using PostSharp.Serialization;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace WebAPI.Aspects
{
    [PSerializable]
    public class PerformanceAspect : OnMethodBoundaryAspect
    {
        private const long SlowExecutionThresholdMs = 1000; // 1 second

        public override void OnEntry(MethodExecutionArgs args)
        {
            args.MethodExecutionTag = Stopwatch.StartNew();
        }

        public override void OnExit(MethodExecutionArgs args)
        {
            var stopwatch = (Stopwatch)args.MethodExecutionTag;
            stopwatch.Stop();

            var logger = ResolveLogger(args);
            var methodName = $"{args.Method.DeclaringType.FullName}.{args.Method.Name}";
            var executionTime = stopwatch.ElapsedMilliseconds;

            logger.LogInformation($"Method {methodName} executed in {executionTime} ms");

            if (executionTime > SlowExecutionThresholdMs)
            {
                logger.LogWarning($"Slow execution detected in method {methodName}. Execution time: {executionTime} ms");
            }

            // If the method returns a Task, we need to ensure it's completed
            if (args.ReturnValue is Task task)
            {
                task.ContinueWith(t => 
                {
                    if (t.Exception != null)
                    {
                        logger.LogError(t.Exception, $"Asynchronous method {methodName} threw an exception");
                    }
                });
            }
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
    }
}