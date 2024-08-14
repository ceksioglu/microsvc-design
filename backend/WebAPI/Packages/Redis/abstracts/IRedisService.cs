using System;
using System.Threading.Tasks;

namespace WebAPI.Packages.Redis.abstracts
{
    public interface IRedisService
    {
        Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task<T> GetAsync<T>(string key);
        Task<bool> DeleteAsync(string key);
        Task<bool> KeyExistsAsync(string key);
        Task<long> ListLeftPushAsync<T>(string key, T value);
        Task<T> ListRightPopAsync<T>(string key);
        Task<bool> HashSetAsync<T>(string key, string hashField, T value);
        Task<T> HashGetAsync<T>(string key, string hashField);
        Task<bool> SetAddAsync<T>(string key, T value);
        Task<bool> SetRemoveAsync<T>(string key, T value);
        Task<bool> SetContainsAsync<T>(string key, T value);
        Task<long> IncrementAsync(string key, long value = 1);
        Task<long> DecrementAsync(string key, long value = 1);
        Task<bool> LockAsync(string key, string value, TimeSpan expiry);
        Task<bool> UnlockAsync(string key, string value);
        Task<TimeSpan?> GetTtlAsync(string key);
        Task<bool> ExpireAsync(string key, TimeSpan expiry);
    }
}