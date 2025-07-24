using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LocationShareApp.API.Data;
using LocationShareApp.API.Models;
using BCrypt.Net;

namespace LocationShareApp.API.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public UserService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<User?> GetUserByPhoneNumberAsync(string phoneNumber)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<User?> GetUserByBindingCodeAsync(string bindingCode)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.BindingCode == bindingCode);
        }

        public async Task<User> CreateUserAsync(string phoneNumber, string password, string nickName)
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            var bindingCode = await GenerateUniqueBindingCodeAsync();

            var user = new User
            {
                PhoneNumber = phoneNumber,
                PasswordHash = passwordHash,
                NickName = nickName,
                BindingCode = bindingCode,
                CreatedAt = DateTime.UtcNow,
                LastActiveAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> ValidatePasswordAsync(User user, string password)
        {
            return await Task.FromResult(BCrypt.Net.BCrypt.Verify(password, user.PasswordHash));
        }

        public async Task<string> GenerateJwtTokenAsync(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
                    new Claim(ClaimTypes.Name, user.NickName)
                }),
                Expires = DateTime.UtcNow.AddHours(Convert.ToDouble(jwtSettings["ExpiryInHours"])),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return await Task.FromResult(tokenHandler.WriteToken(token));
        }

        public async Task<string> GenerateUniqueBindingCodeAsync()
        {
            string bindingCode;
            bool exists;

            do
            {
                bindingCode = GenerateRandomCode();
                exists = await _context.Users.AnyAsync(u => u.BindingCode == bindingCode);
            } while (exists);

            return bindingCode;
        }

        private string GenerateRandomCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public async Task UpdateLastActiveAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.LastActiveAt = DateTime.UtcNow;
                user.IsOnline = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<User>> GetConnectedUsersAsync(int userId)
        {
            return await _context.UserConnections
                .Where(uc => uc.UserId == userId)
                .Include(uc => uc.ConnectedUser)
                .Select(uc => uc.ConnectedUser)
                .ToListAsync();
        }

        public async Task<bool> AddConnectionAsync(int userId, string bindingCode, string nickName)
        {
            var targetUser = await GetUserByBindingCodeAsync(bindingCode);
            if (targetUser == null || targetUser.Id == userId)
                return false;

            var existingConnection = await _context.UserConnections
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ConnectedUserId == targetUser.Id);

            if (existingConnection != null)
                return false;

            var connection = new UserConnection
            {
                UserId = userId,
                ConnectedUserId = targetUser.Id,
                NickName = nickName,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserConnections.Add(connection);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveConnectionAsync(int userId, int connectedUserId)
        {
            var connection = await _context.UserConnections
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ConnectedUserId == connectedUserId);

            if (connection == null)
                return false;

            _context.UserConnections.Remove(connection);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}