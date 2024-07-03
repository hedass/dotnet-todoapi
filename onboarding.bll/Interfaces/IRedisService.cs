namespace onboarding.bll.Interfaces
{
    public interface IRedisService
    {
        public T? GetString<T>(string key);
        public Task<bool> SetStringAsync<T>(string key, T value);
        public Task<bool> RemoveAsync(string key);
    }
}
