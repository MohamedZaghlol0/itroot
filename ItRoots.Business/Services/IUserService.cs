using ItRoots.Business.Dtos;
using ItRoots.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItRoots.Business.Services
{
    public interface IUserService
    {
        Task<int> RegisterAsync(User user);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByIdAsync(int id);
        Task<bool> VerifyEmailAsync(Guid token);
        string HashPassword(string password);
        Task DeleteUserAsync(int id);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task UpdateUserAsync(User user);


    }
}
