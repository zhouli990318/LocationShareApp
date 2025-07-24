using LocationShareApp.API.Models;

namespace LocationShareApp.API.Services
{
    public interface IUserService
    {
        Task<User?> GetUserByPhoneNumberAsync(string phoneNumber);
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByBindingCodeAsync(string bindingCode);
        Task<User> CreateUserAsync(string phoneNumber, string password, string nickName);
        Task<bool> ValidatePasswordAsync(User user, string password);
        Task<string> GenerateJwtTokenAsync(User user);
        Task<string> GenerateUniqueBindingCodeAsync();
        Task UpdateLastActiveAsync(int userId);
        Task<List<User>> GetConnectedUsersAsync(int userId);
        Task<bool> AddConnectionAsync(int userId, string bindingCode, string nickName);
        Task<bool> RemoveConnectionAsync(int userId, int connectedUserId);
    }
}