using Microsoft.AspNetCore.Mvc;
using MoviesAPI.Models.System;

namespace MoviesAPI.Repositories.Interface
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(long id);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<UserProfile?> GetForUpdateAsync(long id);
        Task<long?> GetUserIdByUsernameAsync(string username);
        Task<bool> IsEmailTakenAsync(string email);
        Task<User?> GetByUsernameAndPasswordAsync(string username, string password);
        Task<bool> LogoutAsync(string username);
        Task<int> CreateAsync(RegisterRequest user);
        Task<int> UpdateAsync(UserProfile updateUser);
        Task<int> DeleteAsync(long id);
        Task<bool> UpdatePasswordAsync(int userId, string newPassword);

    }

   
}
