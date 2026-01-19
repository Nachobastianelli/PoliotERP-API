using Application.DTOs.Tenants;

namespace Application.Interfaces.Services;

public interface ITenantManagementService
{
    Task<IEnumerable<TenantDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TenantDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TenantDto> UpdateAsync(int id, UpdateTenantDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<TenantDto> RenewSubscriptionAsync(int id, int months, CancellationToken cancellationToken = default);
}