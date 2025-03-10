using ItRoots.Business.Dtos;
using ItRoots.Data.Models;
using ItRoots.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;


namespace ItRoots.Business.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;

        public UserService(IUserRepository repo)
        {
            _repo = repo;
        }

        public async Task<int> RegisterAsync(User user)
        {
            // Hash the password
            user.PasswordHash = HashPassword(user.PasswordHash);

            // Mark as not verified
            user.IsVerified = false;
            if (string.IsNullOrEmpty(user.Role))
                user.Role = "User";

            // Insert user
            return await _repo.CreateUserAsync(user);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _repo.GetUserByUsernameAsync(username);
        }

        public async Task<bool> VerifyEmailAsync(Guid token)
        {
            var user = await _repo.GetUserByVerificationTokenAsync(token);
            if (user == null) return false;

            user.IsVerified = true;
            // Optionally reset the token: user.VerificationToken = Guid.Empty;
            await _repo.UpdateUserAsync(user);
            return true;
        }

        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
