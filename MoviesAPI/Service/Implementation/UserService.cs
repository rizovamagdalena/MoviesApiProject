using MoviesAPI.Models.System;
using MoviesAPI.Repositories.Interface;
using MoviesAPI.Service.Interface;

namespace MoviesAPI.Service.Implementation
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
            => await _userRepository.GetAllAsync();

        public async Task<User?> GetByIdAsync(long id)
            => await _userRepository.GetByIdAsync(id);

        public async Task<User?> GetByUsernameAsync(string username)
            => await _userRepository.GetByUsernameAsync(username);

        public async Task<User?> GetByEmailAsync(string email)
            => await _userRepository.GetByEmailAsync(email);

        public async Task<bool> IsEmailTakenAsync(string email)
            => await _userRepository.IsEmailTakenAsync(email);

        public async Task<int> CreateAsync(RegisterRequest user)
            => await _userRepository.CreateAsync(user);

        public async Task<int> UpdateAsync(UserProfile user)
        {
            var existing = await _userRepository.GetByUsernameAsync(user.Username);
            if (existing == null)
                throw new KeyNotFoundException($"User '{user.Username}' not found.");

            return await _userRepository.UpdateAsync(user);
        }

        public async Task<int> DeleteAsync(long id)
            => await _userRepository.DeleteAsync(id);

        public async Task<User?> LoginAsync(string username, string password)
            => await _userRepository.GetByUsernameAndPasswordAsync(username, password);

        public async Task<bool> LogoutAsync(string username)
            => await _userRepository.LogoutAsync(username);

        public async Task<int> RegisterAsync(RegisterRequest request)
        {
            if (await _userRepository.IsEmailTakenAsync(request.Email))
                throw new InvalidOperationException("Email already registered.");

            var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingUser != null)
                throw new InvalidOperationException("Username already exists.");

            return await _userRepository.CreateAsync(request);
        }

        public async Task<bool> UpdatePasswordAsync(int userId, string newPassword)
            => await _userRepository.UpdatePasswordAsync(userId, newPassword);
    }
}
