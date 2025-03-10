using ItRoots.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItRoots.Data.Repositories
{
    public interface IUserRepository
    {
        Task<int> CreateUserAsync(User user);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByVerificationTokenAsync(Guid token);
        Task UpdateUserAsync(User user);


    }
}
