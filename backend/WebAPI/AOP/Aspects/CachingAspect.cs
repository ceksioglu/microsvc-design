using PostSharp.Aspects;
using PostSharp.Serialization;
using WebAPI.Packages.Redis.abstracts;
using Newtonsoft.Json;
using System.Reflection;

namespace WebAPI.Aspects
{
    [PSerializable]
    public class CachingAspect : MethodInterceptionAspect
    {
        private readonly TimeSpan _cacheDuration;
        private readonly string _cacheKeyPrefix;

        public CachingAspect(int cacheDurationInSeconds, string cacheKeyPrefix = "")
        {
            _cacheDuration = TimeSpan.FromSeconds(cacheDurationInSeconds);
            _cacheKeyPrefix = cacheKeyPrefix;
        }

        public override void OnInvoke(MethodInterceptionArgs args)
        {
            var redisServiceField = args.Instance.GetType()
                .GetField("_redisService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (redisServiceField == null)
            {
                throw new InvalidOperationException("Redis service field not found.");
            }

            var redisServiceObject = redisServiceField.GetValue(args.Instance);

            if (redisServiceObject is not IRedisService redisService)
            {
                throw new InvalidOperationException("Redis service is not of the expected type.");
            }

            string cacheKey = GenerateCacheKey(args);

            var cachedResult = redisService.GetAsync<string>(cacheKey).Result;
            if (cachedResult != null)
            {
                args.ReturnValue = DeserializeReturnValue(cachedResult, (args.Method as MethodInfo)?.ReturnType);
                return;
            }

            args.Proceed();

            var resultToCache = SerializeReturnValue(args.ReturnValue);
            redisService.SetAsync(cacheKey, resultToCache, _cacheDuration).Wait();
        }

        private string GenerateCacheKey(MethodInterceptionArgs args)
        {
            var methodName = args.Method.DeclaringType.FullName + "." + args.Method.Name;
            var arguments = string.Join("_", args.Arguments.Select(a => a?.ToString() ?? "null"));
            return $"{_cacheKeyPrefix}{methodName}_{arguments}";
        }

        private string SerializeReturnValue(object returnValue)
        {
            if (returnValue is Task task)
            {
                task.Wait();
                var resultProperty = task.GetType().GetProperty("Result");
                returnValue = resultProperty?.GetValue(task);
            }
            return JsonConvert.SerializeObject(returnValue);
        }

        private object DeserializeReturnValue(string cachedResult, Type returnType)
        {
            if (returnType == null)
            {
                throw new InvalidOperationException("Return type could not be determined.");
            }

            var deserializedValue = JsonConvert.DeserializeObject(cachedResult, returnType);
            if (typeof(Task).IsAssignableFrom(returnType))
            {
                return Task.FromResult(deserializedValue);
            }
            return deserializedValue;
        }
    }
}