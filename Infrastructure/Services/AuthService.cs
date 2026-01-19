using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Common;
using Application.DTOs.Auth;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;

public class AuthService: IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IPasswordHasher _hasher;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository userRepository, ITenantRepository tenantRepository, IPasswordHasher hasher,
        IOptions<JwtSettings> jwtSettings, ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _hasher = hasher;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }
    
    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken  = default)
    {
        

        _logger.LogInformation("Starting tenant registration for subdomain {Subdomain}", dto.Subdomain);
        
        if(await _tenantRepository.ExistsSubdomainAsync(dto.Subdomain))
        {
            _logger.LogWarning("Registration failed: Subdomain already exists");
            throw new DuplicateEntityException("Tenant", "Subdomain", dto.Subdomain);
        }
        
        var tenant = new Tenant
        {
            Name = dto.TenantName,
            Subdomain = dto.Subdomain,
            CompanyIdentifier = dto?.CompanyIdentifier,
            IsActive = true,
            SubscriptionEndsAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };
        
        var createdTenant = await _tenantRepository.CreateAsync(tenant);
        _logger.LogInformation("Tenant {Id} ({TenantName}) created with subdomain {Subdomain}",createdTenant.Id, createdTenant.Name, createdTenant.Subdomain);
        
        if(await _userRepository.ExistsUserEmailAsync(dto.Email, cancellationToken))
        { 
            _logger.LogWarning("Registration failed: Email already exists");
            throw new DuplicateEntityException("User", "Email", dto.Email);
        }
        
        if(await _userRepository.ExistsUserUsernameAsync(dto.Username, cancellationToken))
        {
            _logger.LogWarning("Registration failed: Username already exists");
            throw new DuplicateEntityException("User", "Username", dto.Username);
        }

        var user = new User
        {
            TenantId = createdTenant.Id,
            Username = dto.Username,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PasswordHash = _hasher.HashPassword(dto.Password),
            PhoneNumber =  dto.PhoneNumber,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        var createdUser = await _userRepository.CreateUserAsync(user, cancellationToken);
        _logger.LogInformation("Owner user {UserId} created for tenant {TenantId}", createdUser.Id,createdTenant.Id);
        
        // TODO: Crear rol "Owner" y asignarlo al usuario
        // Por ahora generamos el token sin roles

        // Generar JWT
        var token = GenerateJwtToken(createdUser, createdTenant, new List<string> { "Owner" }, new List<string>());
        
        _logger.LogInformation("Registration completed successfully for tenant {TenantId}, user {UserId}",createdTenant.Id, createdUser.Id);
        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(_jwtSettings.ExpirationInHours),
            User = new UserInfoDto
            {
                Id = createdUser.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                TenantId = createdTenant.Id,
                TenantName = createdTenant.Name,
                Roles = new List<string> { "Owner" },
                Permissions = new List<string>() // Todo: Agregar los permisos rales
            }
        };


    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {

        _logger.LogInformation("Login attempt");
        var user = await _userRepository.GetUserByEmailGlobalAsync(dto.Email, cancellationToken);
        
        var passwordHash = user?.PasswordHash ?? "$2a$12$DummyHashToPreventTimingAttack";
        var isPasswordValid = _hasher.VerifyHashedPassword(dto.Password, passwordHash);
        
        if(user == null || !isPasswordValid)
        {
            _logger.LogWarning("Failed login attempt");
            throw new UnauthorizedException("Invalid email or password");
        }

        if(!user.IsActive)
        {
            _logger.LogWarning("Failed login attempt");
            throw new UnauthorizedException("User account is inactive");
        }
        var tenant = user.Tenant ?? await _tenantRepository.GetByIdAsync(user.TenantId);
        
        if(tenant == null || !tenant.IsActive )
        {
            _logger.LogWarning("Failed login attempt");
            throw new UnauthorizedException("Organization is inactive");
        }
        
        if (tenant.SubscriptionEndsAt is DateTime subscriptionEnd && subscriptionEnd < DateTime.UtcNow)
        {
            _logger.LogWarning("Failed login attempt");
            tenant.IsActive = false;
            tenant.UpdatedAt = DateTime.UtcNow;
            await _tenantRepository.UpdateAsync(tenant);
            throw new SubscriptionExpiredException(subscriptionEnd, tenant.Name);
        }
        
        // TODO: Obtener roles y permisos reales del usuario
        var roles = new List<string> { "Owner" }; // Hardcodeado por ahora
        var permissions = new List<string>(); // Vacío por ahora

        // Generar JWT
        var token = GenerateJwtToken(user, tenant, roles, permissions);
        _logger.LogInformation("Successful login for user {UserId}, tenant {TenantId}", user.Id, user.TenantId);
        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(_jwtSettings.ExpirationInHours),
            User = new UserInfoDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                TenantId = tenant.Id,
                TenantName = tenant.Name,
                Roles = roles,
                Permissions = permissions
            }
        };
    }
    
    private string GenerateJwtToken(User user, Tenant tenant, List<string> roles, List<string> permissions)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            new("userId", user.Id.ToString()),
            new("tenantId", tenant.Id.ToString()),
            new("username", user.Username),
            new("fullName", $"{user.FirstName} {user.LastName}"),
            new("tenantName", tenant.Name)
        };

        // Agregar roles
        foreach (var role in roles)
        {
            claims.Add(new Claim("role", role));
        }

        // Agregar permisos
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_jwtSettings.ExpirationInHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}