using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IUserRepository
{
    // Read operations
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<User?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default);
    
    // Global search (for login)
    Task<User?> GetUserByEmailGlobalAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);
    
    // Write operations
    Task<User> CreateUserAsync(User user, CancellationToken cancellationToken = default);
    Task<User> UpdateUserAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(int id, CancellationToken cancellationToken = default);
    
    // Validations
    Task<bool> ExistsUserEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsUserUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> ExistUserAsync(int id, CancellationToken cancellationToken = default);

}