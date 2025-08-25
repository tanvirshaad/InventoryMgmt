using System;
using System.Threading.Tasks;
using InventoryMgmt.DAL.EF.TableModels;
using InventoryMgmt.DAL.Interfaces;

namespace InventoryMgmt.BLL.Services
{
    public interface IAuthService
    {
        Task<(bool success, string token, string message)> LoginAsync(string email, string password);
        Task<(bool success, string message)> RegisterAsync(string userName, string email, string password, string firstName, string lastName);
        Task<User?> GetCurrentUserAsync(string userId);
        Task<bool> IsUserInRoleAsync(int userId, string role);
        Task<User?> GetUserByEmailAsync(string email);
    }

    public class AuthService : IAuthService
    {
        private readonly IRepo<User> _userRepo;
        private readonly IJwtService _jwtService;

        public AuthService(IRepo<User> userRepo, IJwtService jwtService)
        {
            _userRepo = userRepo;
            _jwtService = jwtService;
        }

        public async Task<(bool success, string token, string message)> LoginAsync(string email, string password)
        {
            try
            {
                var user = await _userRepo.FindFirstAsync(u => u.Email == email && !u.IsBlocked);
                if (user == null)
                {
                    return (false, "", "Invalid email or password");
                }

                if (!_jwtService.VerifyPassword(password, user.PasswordHash))
                {
                    return (false, "", "Invalid email or password");
                }

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                _userRepo.Update(user);
                await _userRepo.SaveChangesAsync();

                var token = _jwtService.GenerateToken(user);
                return (true, token, "Login successful");
            }
            catch (Exception ex)
            {
                return (false, "", $"Login failed: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> RegisterAsync(string userName, string email, string password, string firstName, string lastName)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userRepo.FindFirstAsync(u => u.Email == email || u.UserName == userName);
                if (existingUser != null)
                {
                    return (false, "User with this email or username already exists");
                }

                var user = new User
                {
                    UserName = userName,
                    Email = email,
                    PasswordHash = _jwtService.HashPassword(password),
                    FirstName = firstName,
                    LastName = lastName,
                    Role = "User",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow
                };

                await _userRepo.AddAsync(user);
                await _userRepo.SaveChangesAsync();
                return (true, "Registration successful");
            }
            catch (Exception ex)
            {
                return (false, $"Registration failed: {ex.Message}");
            }
        }

        public async Task<User?> GetCurrentUserAsync(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int id))
                {
                    return null;
                }

                return await _userRepo.GetByIdAsync(id);
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> IsUserInRoleAsync(int userId, string role)
        {
            try
            {
                var user = await _userRepo.GetByIdAsync(userId);
                return user?.Role == role;
            }
            catch
            {
                return false;
            }
        }
        
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return null;
                }
                
                return await _userRepo.FindFirstAsync(u => u.Email == email);
            }
            catch
            {
                return null;
            }
        }
    }
}
