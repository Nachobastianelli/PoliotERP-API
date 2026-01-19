using Application.DTOs.Tenants;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class TenantManagementService  : ITenantManagementService
{
    private readonly ITenantService _tenantService;
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<TenantManagementService> _logger;
    
    public TenantManagementService(ITenantService tenantService, ITenantRepository tenantRepository, ILogger<TenantManagementService> logger)
    {
        _tenantService = tenantService;
        _tenantRepository = tenantRepository;
        _logger = logger;
    }
    public async Task<IEnumerable<TenantDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (!_tenantService.IsSuperAdmin())
        {
            _logger.LogWarning("Non-SuperAdmin user attempted to view all tenants");
            throw new ForbiddenException("You cant make this action");
        }

        _logger.LogInformation("SuperAdmin fetching all tenants");
        var tenants = await _tenantRepository.GetAllAsync(cancellationToken);
        _logger.LogInformation("Retrieved {Count} tenants", tenants.Count());
        return tenants.Select(MapToDto);
    }

    public async Task<TenantDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching tenant {TenantId}", id);
        var tenant =  await _tenantRepository.GetByIdAsync(id, cancellationToken);

        if (tenant == null)
        {
            _logger.LogWarning("Tenant {TenantId} not found", id);
            throw new EntityNotFoundException("Tenant", id);
        }
        
        if (!_tenantService.IsSuperAdmin())
        {
            var currentTenantId = _tenantService.GetTenantId();
            if (tenant.Id != currentTenantId)
            {
                _logger.LogWarning("User from tenant {CurrentTenantId} attempted to view tenant {RequestedTenantId}",currentTenantId,id); 
                throw new ForbiddenException("You cant make this action");
            }
        }

        return MapToDto(tenant);

    }

    public async Task<TenantDto> UpdateAsync(int id, UpdateTenantDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating tenant {TenantId}", id);
        var tenant = await _tenantRepository.GetByIdAsync(id, cancellationToken);
        
        if (tenant == null)
        {
            _logger.LogWarning("Attempt to update non-existent tenant {TenantId}", id);
            throw new EntityNotFoundException("Tenant", id);
        }

        var isSuperAdmin = _tenantService.IsSuperAdmin();

        if (!isSuperAdmin)
        {
            var currentTenantId = _tenantService.GetTenantId();
            if (tenant.Id != currentTenantId)
            {
                _logger.LogWarning("User from tenant {CurrentTenantId} attempted to update tenant {TargetTenantId}", 
                    currentTenantId, id);
                throw new ForbiddenException("You cant make this action");
            }

            if (dto.IsActive.HasValue || dto.SubscriptionEndsAt.HasValue)
            {
                _logger.LogWarning("Non-SuperAdmin user attempted to modify subscription/active status for tenant {TenantId}", id);
                throw new ForbiddenException("You cant make this action");
            }
        }
        
        if(!string.IsNullOrWhiteSpace(dto.Name)) tenant.Name = dto.Name;
        
        if(!string.IsNullOrWhiteSpace(dto.CompanyIdentifier))  tenant.CompanyIdentifier = dto.CompanyIdentifier;

        if (await _tenantRepository.ExistsSubdomainAsync(tenant.Subdomain, cancellationToken))
        {
            throw new DuplicateEntityException("Tenant","Subdomain", tenant.Subdomain);
        }
        
        if(!string.IsNullOrWhiteSpace(dto.Subdomain))  tenant.Subdomain = dto.Subdomain;

        if (isSuperAdmin)
        {
            if(dto.IsActive.HasValue) tenant.IsActive = dto.IsActive.Value;
            if(dto.SubscriptionEndsAt.HasValue) tenant.SubscriptionEndsAt = dto.SubscriptionEndsAt;
        }
        
        tenant.UpdatedAt = DateTime.UtcNow;
        
        var updatedTenant = await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        _logger.LogInformation("Successfully updated tenant {TenantId}", id);
        
        return MapToDto(updatedTenant);
    }

    public async Task<bool> DeleteAsync(int id,  CancellationToken cancellationToken = default)
    {
        var isSuperAdmin = _tenantService.IsSuperAdmin();
        if (!isSuperAdmin)
        {
            _logger.LogWarning("Non-SuperAdmin user attempted to delete tenant {TenantId}", id);
            throw new ForbiddenException("You cant make this action");
        }
        
        _logger.LogInformation("SuperAdmin deleting tenant {TenantId}", id);

        var tenant = await _tenantRepository.ExistsAsync(id, cancellationToken);
        
        if (!tenant)
            throw new EntityNotFoundException("Tenant", id);
        
        var result = await _tenantRepository.DeleteAsync(id,  cancellationToken);
        
        if(result)
            _logger.LogInformation("Successfully deleted tenant {TenantId}", id);
        else
            _logger.LogWarning("Tenant {TenantId} not found for deletion", id);
        return result;
    }

    public async Task<TenantDto> RenewSubscriptionAsync(int id, int months, CancellationToken cancellationToken = default)
    {
        var isSuperAdmin = _tenantService.IsSuperAdmin();
        if (!isSuperAdmin)
        {
            _logger.LogWarning("Non-SuperAdmin user attempted to renew subscription for tenant {TenantId}", id);
            throw new ForbiddenException("You cant make this action");
        }
        
        _logger.LogInformation("SuperAdmin renewing subscription for tenant {TenantId} by {Months} months", id, months);

        var tenant = await _tenantRepository.GetByIdAsync(id, cancellationToken);
        
        if (tenant == null)
        {
            _logger.LogWarning("Attempt to renew subscription for non-existent tenant {TenantId}", id);
            throw new EntityNotFoundException("Tenant", id);
        }
        var baseDate = tenant.SubscriptionEndsAt.HasValue && tenant.SubscriptionEndsAt.Value > DateTime.UtcNow
            ? tenant.SubscriptionEndsAt.Value
            : DateTime.UtcNow;
        
        tenant.SubscriptionEndsAt = baseDate.AddMonths(months);
        tenant.IsActive = true;
        tenant.UpdatedAt = DateTime.UtcNow;
        
        var renewedTenant = await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        _logger.LogInformation("Successfully renewed subscription for tenant {TenantId} until {ExpirationDate}", 
            id, tenant.SubscriptionEndsAt);
        
        return MapToDto(renewedTenant);
        
    }
    
    private static TenantDto MapToDto(Tenant tenant)
    {
        return new TenantDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Subdomain = tenant.Subdomain,
            CompanyIdentifier = tenant.CompanyIdentifier,
            IsActive = tenant.IsActive,
            SubscriptionEndsAt = tenant.SubscriptionEndsAt,
            CreatedAt = tenant.CreatedAt,
            UpdatedAt = tenant.UpdatedAt
        };
    }
}