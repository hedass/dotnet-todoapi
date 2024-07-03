using Microsoft.Extensions.Caching.Distributed;
using onboarding.bll.Interfaces;
using System.Text.Json;

namespace onboarding.bll.Services
{
    public class RedisService : IRedisService
    {

        private readonly IDistributedCache _cache;

        public RedisService(IDistributedCache cache) 
        { 
            _cache = cache; 
        }

        public T? GetString<T>(string key)
        {
            try
            {
                string stringValue = _cache.GetString(key);
                if (stringValue == null)
                {
                    return default;
                }
                else
                {
                    var objectValue = JsonSerializer.Deserialize<T>(stringValue);
                    return objectValue;
                }
            }
            catch
            {
                return default;
            }
        }

        public async Task<bool> RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SetStringAsync<T>(string key, T value )
        {
            string serializedValue = JsonSerializer.Serialize(value);

            try
            {
                await _cache.SetStringAsync(key, serializedValue);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
