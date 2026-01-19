using Application.DTOs;

namespace Application.Interfaces.Services;

public interface IUserService
{
    Task<UserDto> CreateUserAsync(CreateUserDto createUserDto, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<UserDto> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsUserAsync(int id, CancellationToken cancellationToken = default);
}