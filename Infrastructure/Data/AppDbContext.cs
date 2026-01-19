using System.Linq.Expressions;
using System.Reflection;
using Application.Interfaces.Services;
using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class AppDbContext : DbContext
{

    private readonly ITenantService? _tenantService;
    
    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantService? tenantService = null)
        : base(options)
    {
        _tenantService = tenantService;
    }
    
    //DbSets
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        //Apply entity config
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        ApplyGlobalFilters(modelBuilder);
    }

    private void ApplyGlobalFilters(ModelBuilder modelBuilder)
    {
        // Global filter for soft delete: automatically filters out deleted records (WHERE DeletedAt IS NULL)
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                // Create parameter: (e)
                var parameter = Expression.Parameter(entityType.ClrType, "e");
        
                // Access property: (e.DeletedAt)
                var property = Expression.Property(parameter, nameof(BaseEntity.DeletedAt));
        
                // Create condition: (e.DeletedAt == null) - only show non-deleted records
                var condition = Expression.Equal(property, Expression.Constant(null, typeof(DateTime?)));
        
                // Build lambda expression: e => e.DeletedAt == null
                var lambda = Expression.Lambda(condition, parameter);
        
                // Apply filter to entity
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }

        if (_tenantService != null)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(TenantEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var method =  SetGlobalQueryForTenantMethodInfo.MakeGenericMethod(entityType.ClrType);
                    method.Invoke(this, new object[] { modelBuilder });
                }
            }
        }
    }
    
    
    private static readonly MethodInfo SetGlobalQueryForTenantMethodInfo =
        typeof(AppDbContext)
            .GetMethod(nameof(SetGlobalQueryForTenant), BindingFlags.NonPublic | BindingFlags.Instance)!; 

    private void SetGlobalQueryForTenant<T>(ModelBuilder modelBuilder)  where T : TenantEntity
    {
        modelBuilder.Entity<T>().HasQueryFilter(e =>
            e.DeletedAt == null &&
            e.TenantId == _tenantService!.GetTenantId());
    }
    
   
    // Intercepts SaveChanges to automatically populate audit fields and handle soft deletes
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        var currentUserId = _tenantService?.GetUserId();
        
        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = currentUserId;
                    break;
                
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = currentUserId;
                    break;
                
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    entry.Entity.DeletedBy = currentUserId;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}