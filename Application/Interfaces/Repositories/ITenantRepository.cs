using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Tenant?> GetBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tenant>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Tenant> CreateAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task<Tenant> UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsSubdomainAsync(string subdomain, CancellationToken cancellationToken = default);
}