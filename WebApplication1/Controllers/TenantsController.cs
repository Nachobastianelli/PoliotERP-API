using System.ComponentModel.DataAnnotations;
using Application.DTOs.Tenants;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TenantsController : ControllerBase
{
    private readonly ITenantManagementService _tenantService;
    private readonly ILogger<TenantsController> _logger;

    public TenantsController(ITenantManagementService tenantService, ILogger<TenantsController> logger)
    {
        _tenantService = tenantService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TenantDto>>> GetAll(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching all tenants");
        var tenants = await _tenantService.GetAllAsync(cancellationToken);
        return Ok(tenants);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TenantDto>> GetById(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching tenant with id {id}",id);
        var tenant = await _tenantService.GetByIdAsync(id,cancellationToken);
        return Ok(tenant);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TenantDto>> Update(int id, [FromBody] UpdateTenantDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _logger.LogInformation("Updating tenant {id}", id);
        var tenant = await _tenantService.UpdateAsync(id, dto,cancellationToken);
        _logger.LogInformation("Successfully updated tenant {id}", id);
        return Ok(tenant);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> Delete(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attempting to delete tenant {TenantId}", id);
        var deleted = await _tenantService.DeleteAsync(id, cancellationToken);

        if (!deleted)
        {   
            _logger.LogWarning("Tenant {TenantId} not found for deletion", id);
            return NotFound($"The entity with id {id} was not found.");
        }

        _logger.LogInformation("Successfully deleted tenant {TenantId}", id);
        return NoContent();
    }

    [HttpPost("{id}/renew")]
    public async Task<ActionResult<TenantDto>> RenewSubscription(int id, [FromBody] RenewSubscriptionDto dto, CancellationToken cancellationToken)
    {
        
        if(!ModelState.IsValid)
            return BadRequest(ModelState);
        _logger.LogInformation("Renewing subscription for tenant {TenantId} by {Months} months",id,dto.Months);
        var tenant = await _tenantService.RenewSubscriptionAsync(id, dto.Months, cancellationToken);
        _logger.LogInformation("Successfully renewed subscription for tenant {TenantId}", id);
        return Ok(tenant);
            
    }
    
    public class RenewSubscriptionDto
    {
        [Range(1,36, ErrorMessage =  "Months must be between 1 and 36")]
        public int Months { get; set; }
    }

}