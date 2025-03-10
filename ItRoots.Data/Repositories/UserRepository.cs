using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItRoots.Data.Models;
using Microsoft.Data.SqlClient;
using Dapper;

namespace ItRoots.Data.Repositories
{
    public class UserRepository : IUserRepository
    {

        private readonly IDbConnection _db;

        public UserRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<int> CreateUserAsync(User user)
        {
            var sql = @"
            INSERT INTO Users 
                (FullName, Username, PasswordHash, Email, Phone, Role, IsVerified, VerificationToken)
            VALUES 
                (@FullName, @Username, @PasswordHash, @Email, @Phone, @Role, @IsVerified, @VerificationToken);

            SELECT CAST(SCOPE_IDENTITY() as int);
        ";
            return await _db.ExecuteScalarAsync<int>(sql, user);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            var sql = "SELECT * FROM Users WHERE Username = @username";
            return await _db.QueryFirstOrDefaultAsync<User>(sql, new { username });
        }

        public async Task<User?> GetUserByVerificationTokenAsync(Guid token)
        {
            var sql = "SELECT * FROM Users WHERE VerificationToken = @token";
            return await _db.QueryFirstOrDefaultAsync<User>(sql, new { token });
        }

        public async Task UpdateUserAsync(User user)
        {
            var sql = @"
            UPDATE Users
            SET 
                FullName = @FullName,
                Username = @Username,
                PasswordHash = @PasswordHash,
                Email = @Email,
                Phone = @Phone,
                Role = @Role,
                IsVerified = @IsVerified,
                VerificationToken = @VerificationToken
            WHERE 
                Id = @Id
        ";
            await _db.ExecuteAsync(sql, user);
        }

        public async Task DeleteUserAsync(int id)
        {
            var sql = "DELETE FROM Users WHERE Id = @Id;";
            await _db.ExecuteAsync(sql, new { Id = id });
        }
        public async Task<User?> GetUserByIdAsync(int id)
        {
            var sql = "SELECT * FROM Users WHERE Id = @Id;";
            return await _db.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            var sql = "SELECT * FROM Users;";
            return await _db.QueryAsync<User>(sql);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var sql = "SELECT * FROM Users WHERE Email = @Email;";
            return await _db.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
        }

        
    }
}