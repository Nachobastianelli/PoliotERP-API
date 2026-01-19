using System.Diagnostics;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(t => t.Subdomain)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.CompanyIdentifier)
            .HasMaxLength(50);

        builder.Property(t => t.SubscriptionEndsAt)
            .IsRequired();
    
        builder.Property(t => t.IsActive)
            .HasDefaultValue(true);


        builder.HasIndex(t => t.Subdomain)
            .IsUnique()
            .HasDatabaseName("IX_Tenants_Subdomain");
        
    }
}