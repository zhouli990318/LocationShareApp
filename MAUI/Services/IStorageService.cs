namespace LocationShareApp.Services
{
    public interface IStorageService
    {
        Task<string?> GetAsync(string key);
        Task SetAsync(string key, string value);
        Task RemoveAsync(string key);
        Task<bool> ContainsKeyAsync(string key);
        Task ClearAsync();
    }
}