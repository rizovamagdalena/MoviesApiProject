using Microsoft.AspNetCore.Mvc;
using MoviesAPI.Models.System;

namespace MoviesAPI.Repositories.Interface
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetUsersAsync();
        Task<User> GetUserAsync(long id);
        Task<User> GetUserByUsername(string username);
        Task<int> CreateUserAsync(RegisterRequest user); 
        Task<int> UpdateUserAsync(long id, UpdateUser user);
        Task<UpdateUser> GetUserForUpdateAsync(long id);
        Task<int> DeleteUserAsync(long id);
        Task<User> GetUserByUsernameAndPassword(string username, string password);
        Task<bool> LogoutUser(string username);
    }

   
}
