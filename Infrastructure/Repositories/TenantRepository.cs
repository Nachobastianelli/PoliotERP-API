using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly AppDbContext _context;
    
    public TenantRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<Tenant?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants.FindAsync(id, cancellationToken);
    }

    public async Task<Tenant?> GetBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default)
    {
        return  await _context.Tenants.FirstOrDefaultAsync(t => t.Subdomain == subdomain, cancellationToken);
    }

    public async Task<IEnumerable<Tenant>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tenants.OrderBy(t => t.Name).ToListAsync(cancellationToken);
    }

    public async Task<Tenant> CreateAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync(cancellationToken);
        return tenant;
    }

    public async Task<Tenant> UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        _context.Tenants.Update(tenant);
        await _context.SaveChangesAsync(cancellationToken);
        return tenant;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var tenant = await GetByIdAsync(id, cancellationToken);
        
        if(tenant == null) return false;

        tenant.IsActive = false;
        tenant.DeletedAt= DateTime.UtcNow;
        //tenant.DeleteBy -> Esto se le da en el service (Extraer del jwt)
        
        await _context.SaveChangesAsync(cancellationToken);
        return true;

    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants.AnyAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsSubdomainAsync(string subdomain, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants.AnyAsync(t => t.Subdomain == subdomain, cancellationToken);
    }
}