using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Kanini.Domain.Entities;

namespace Kanini.Data.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.HasKey(o => o.OrganizationId);
        builder.HasIndex(o => o.OrganizationId).IsUnique();
        builder.Property(o => o.Name).HasMaxLength(500);
        builder.Property(o => o.ContactEmail).HasMaxLength(300);
        builder.Property(o => o.ContactPhone).HasMaxLength(200);
    }
}