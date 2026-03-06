using MoviesAPI.Models.System;

namespace MoviesAPI.Service.Interface
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(long id);
        Task<int> CreateAsync(RegisterRequest user);
        Task<int> UpdateAsync(UserProfile user);
        Task<int> DeleteAsync(long id);
        Task<User?> LoginAsync(string username, string password);
        Task<bool> LogoutAsync(string username);
        Task<int> RegisterAsync(RegisterRequest request);
        Task<bool> UpdatePasswordAsync(int userId, string newPassword);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<bool> IsEmailTakenAsync(string email);
    }
}
