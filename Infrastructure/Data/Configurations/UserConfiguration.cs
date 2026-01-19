using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(320);
        
        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.PhoneNumber)
            .IsRequired()
            .HasMaxLength(25);

        builder.Property(u => u.IsActive)
            .HasDefaultValue(true);
        
        
        //Constrains en Database -> Email e Username unicos por tenantId
        builder.HasIndex(u => new {u.Email, u.TenantId})
            .IsUnique()
            .HasDatabaseName("IX_Users_Email_TenantId");
        
        builder.HasIndex(u => new { u.Username, u.TenantId})
            .IsUnique()
            .HasDatabaseName("IX_Users_Username_TenantId");
        
        //Relacion con el tenant
        builder.HasOne(u => u.Tenant)
            .WithMany(t => t.Users)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}