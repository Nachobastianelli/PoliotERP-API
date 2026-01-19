using Application.DTOs.Auth;

namespace Application.Interfaces.Services;

public interface IAuthService
{
    /// <summary>
    /// Registra un nuevo tenant con su usuario admin
    /// </summary>
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Inicia sesión y devuelve un JWT
    /// </summary>
    Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
}