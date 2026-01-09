using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Kanini.Domain.Entities;

namespace Kanini.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.UserId);
        builder.HasIndex(u => u.Email).IsUnique();
        
        // User → Organization relationship
        builder.HasOne(u => u.Organization)
               .WithMany(o => o.Users)
               .HasForeignKey(u => u.OrganizationId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}