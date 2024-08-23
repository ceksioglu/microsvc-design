using Core.Redis.abstracts;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Core.Redis.concretes
{
    public class RedisService : IRedisService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        public RedisService(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _db = _redis.GetDatabase();
        }

        public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            string serializedValue = JsonConvert.SerializeObject(value);
            return await _db.StringSetAsync(key, serializedValue, expiry);
        }

        public async Task<T> GetAsync<T>(string key)
        {
            var value = await _db.StringGetAsync(key);
            if (value.IsNullOrEmpty)
                return default;
            return JsonConvert.DeserializeObject<T>(value);
        }

        public async Task<bool> DeleteAsync(string key)
        {
            return await _db.KeyDeleteAsync(key);
        }

        public async Task<bool> KeyExistsAsync(string key)
        {
            return await _db.KeyExistsAsync(key);
        }

        public async Task<long> ListLeftPushAsync<T>(string key, T value)
        {
            string serializedValue = JsonConvert.SerializeObject(value);
            return await _db.ListLeftPushAsync(key, serializedValue);
        }

        public async Task<T> ListRightPopAsync<T>(string key)
        {
            var value = await _db.ListRightPopAsync(key);
            if (value.IsNullOrEmpty)
                return default;
            return JsonConvert.DeserializeObject<T>(value);
        }

        public async Task<bool> HashSetAsync<T>(string key, string hashField, T value)
        {
            string serializedValue = JsonConvert.SerializeObject(value);
            return await _db.HashSetAsync(key, hashField, serializedValue);
        }

        public async Task<T> HashGetAsync<T>(string key, string hashField)
        {
            var value = await _db.HashGetAsync(key, hashField);
            if (value.IsNullOrEmpty)
                return default;
            return JsonConvert.DeserializeObject<T>(value);
        }

        public async Task<bool> SetAddAsync<T>(string key, T value)
        {
            string serializedValue = JsonConvert.SerializeObject(value);
            return await _db.SetAddAsync(key, serializedValue);
        }

        public async Task<bool> SetRemoveAsync<T>(string key, T value)
        {
            string serializedValue = JsonConvert.SerializeObject(value);
            return await _db.SetRemoveAsync(key, serializedValue);
        }

        public async Task<bool> SetContainsAsync<T>(string key, T value)
        {
            string serializedValue = JsonConvert.SerializeObject(value);
            return await _db.SetContainsAsync(key, serializedValue);
        }

        public async Task<long> IncrementAsync(string key, long value = 1)
        {
            return await _db.StringIncrementAsync(key, value);
        }

        public async Task<long> DecrementAsync(string key, long value = 1)
        {
            return await _db.StringDecrementAsync(key, value);
        }

        public async Task<bool> LockAsync(string key, string value, TimeSpan expiry)
        {
            return await _db.LockTakeAsync(key, value, expiry);
        }

        public async Task<bool> UnlockAsync(string key, string value)
        {
            return await _db.LockReleaseAsync(key, value);
        }

        public async Task<TimeSpan?> GetTtlAsync(string key)
        {
            return await _db.KeyTimeToLiveAsync(key);
        }

        public async Task<bool> ExpireAsync(string key, TimeSpan expiry)
        {
            return await _db.KeyExpireAsync(key, expiry);
        }
    }
}