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

        public async Task DeleteUserAsync(int id)
        {
            // (Optional) You can first verify the user exists:
            var existingUser = await _repo.GetUserByIdAsync(id);
            if (existingUser == null)
                throw new Exception("User not found.");

            // Then call repo to delete:
            await _repo.DeleteUserAsync(id);
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            // (Optional) You can first verify the user exists:
            var existingUser = await _repo.GetUserByIdAsync(id);

            if (existingUser == null)
                throw new Exception("User not found.");

            // Then call repo to delete:
            return existingUser;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _repo.GetAllUsersAsync();
        }

        public async Task UpdateUserAsync(User updatedUser)
        {
            // Retrieve the existing user from the repository
            var existingUser = await _repo.GetUserByIdAsync(updatedUser.Id);
            if (existingUser == null)
            {
                throw new Exception("User not found.");
            }

            // Update only the fields that are provided in the update form
            existingUser.FullName = updatedUser.FullName;
            existingUser.Username = updatedUser.Username;
            existingUser.Email = updatedUser.Email;
            existingUser.Role = updatedUser.Role;
            existingUser.IsVerified = updatedUser.IsVerified;

            // Optionally update Phone if your modal had that field.
            // For example, if you add a phone input in the modal:
            // existingUser.Phone = updatedUser.Phone;

            // Do not update PasswordHash unless a new password is provided.
            // (Assuming your modal does NOT have a password field.)
            if (!string.IsNullOrWhiteSpace(updatedUser.PasswordHash))
            {
                // If a new password is provided, hash and update it.
                existingUser.PasswordHash = HashPassword(updatedUser.PasswordHash);
            }
            // Otherwise, preserve the existing PasswordHash.

            // Likewise, you can decide what to do with VerificationToken:
            // For now, we leave it unchanged.

            // Now pass the updated existing user to the repository update
            await _repo.UpdateUserAsync(existingUser);
        }



    }
}
