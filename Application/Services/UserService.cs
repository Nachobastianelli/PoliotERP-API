using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITenantService _tenantService;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher, ITenantService tenantService, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tenantService = tenantService;
        _logger = logger;
    }
    
    public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantService.GetTenantId();
        _logger.LogInformation("Creating user with Email {Email} for tenant {TenantId}",createUserDto.Email, tenantId);
        
        //Valido que no exista el email en DB:
        if (await _userRepository.ExistsUserEmailAsync(createUserDto.Email, cancellationToken))
        {
            _logger.LogWarning("Attempt to create user with duplicate email {email}", createUserDto.Email);
            throw new DuplicateEntityException("User", "Email", createUserDto.Email);
        }
        
        if (await _userRepository.ExistsUserUsernameAsync(createUserDto.Username))
        {
            _logger.LogWarning("Attempt to create user with duplicate username {Username}",createUserDto.Username);
            throw new DuplicateEntityException("User", "Username", createUserDto.Username);
        }
        
        var passwordHashed = _passwordHasher.HashPassword(createUserDto.Password);
        
        var user = new User
        {
            TenantId = tenantId,
            Email = createUserDto.Email,
            Username = createUserDto.Username,
            FirstName = createUserDto.FirstName,
            LastName = createUserDto.LastName,
            PhoneNumber = createUserDto.PhoneNumber,
            PasswordHash = passwordHashed,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        var createdUser = await _userRepository.CreateUserAsync(user, cancellationToken);
        
        _logger.LogInformation("Successfully created user {UserId} {Username} for tenant {TenantId}",createdUser.Id, createdUser.Username, tenantId);
        return MapToDto(createdUser);
    }

    public async Task<UserDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating user {Id}", id);
        var user = await _userRepository.GetUserByIdAsync(id, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("Attempt to update non-exist user {UserId}", id);
            throw new EntityNotFoundException("User", id);
        }

        if (!string.IsNullOrWhiteSpace(updateUserDto.Email))
        {
            var existingUser = await _userRepository.GetUserByEmailAsync(updateUserDto.Email, cancellationToken);
            if(existingUser != null && existingUser.Id != id)
            {
                _logger.LogWarning("Attempt to update user {UserId} with duplicate email {Email}",id, updateUserDto.Email);
                throw new DuplicateEntityException("User", "Email", updateUserDto.Email);
            }
            user.Email = updateUserDto.Email;
        }

        if (!string.IsNullOrWhiteSpace(updateUserDto.Username))
        {
            var existingUser = await _userRepository.GetUserByUsernameAsync(updateUserDto.Username, cancellationToken);
            if (existingUser != null && existingUser.Id != id)
            {
                _logger.LogWarning("Attempt to update user {UserId} with duplicate username {Username}",id, updateUserDto.Username);
                throw new DuplicateEntityException("User", "Username", updateUserDto.Username);
            }
            user.Username = updateUserDto.Username;
        }
        
        if(!string.IsNullOrWhiteSpace(updateUserDto.FirstName)) user.FirstName = updateUserDto.FirstName;
        if (!string.IsNullOrWhiteSpace(updateUserDto.LastName)) user.LastName = updateUserDto.LastName;
        if (!string.IsNullOrWhiteSpace(updateUserDto.PhoneNumber)) user.PhoneNumber = updateUserDto.PhoneNumber;
        if(updateUserDto.IsActive.HasValue) user.IsActive = updateUserDto.IsActive.Value;
        user.UpdatedAt = DateTime.UtcNow;
        
        var updatedUser = await _userRepository.UpdateUserAsync(user, cancellationToken);
        
        _logger.LogInformation("Successfully updated user {UserId} for tenant {TenantId}",updatedUser.Id, updatedUser.TenantId);
        return MapToDto(updatedUser);
    }

    public async  Task<IEnumerable<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantService.GetTenantId();
        _logger.LogDebug("Fetching all users for tenant {TenantId}", tenantId);
        
        var users = await _userRepository.GetAllAsync(cancellationToken);

        _logger.LogDebug("Retrieved {Count} users for tenant {TenantId}", users.Count(),tenantId);
        return users.Select(MapToDto);
    }

    public async Task<UserDto> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching user {Id}", id);
        
        var user = await _userRepository.GetUserByIdAsync(id, cancellationToken);
        
        if(user == null)
        {
            _logger.LogWarning("User with ID {Id} not found", id);
            throw new EntityNotFoundException("User", id);
        }
        
        return  MapToDto(user); 
    }

    
    public async Task<bool> DeleteUserAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Deleting user {Id}", id);
        
        var result = await _userRepository.DeleteUserAsync(id, cancellationToken);
        
        if(result)
            _logger.LogInformation("Successfully deleted user {Id}", id);
        
        else
            _logger.LogWarning("User {UserId} not found for deletion", id);   
        
        return result;
    }

    public async Task<bool> ExistsUserAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _userRepository.ExistUserAsync(id, cancellationToken); 
    }
    
    // Métodos privados auxiliares: Excluye la contrasenia del cuerpo de la respuesta
    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            TenantId = user.TenantId,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}