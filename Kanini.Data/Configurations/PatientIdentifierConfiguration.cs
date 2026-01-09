using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Kanini.Domain.Entities;

namespace Kanini.Data.Configurations;

public class PatientIdentifierConfiguration : IEntityTypeConfiguration<PatientIdentifier>
{
    public void Configure(EntityTypeBuilder<PatientIdentifier> builder)
    {
        builder.HasKey(p => p.Id);
        
        // Indexes
        builder.HasIndex(p => new { p.GlobalPatientId, p.SourceOrganizationId }).IsUnique();
        builder.HasIndex(p => new { p.LastNameHash, p.DateOfBirthHash });
        
        // Properties
        builder.Property(p => p.LocalPatientId).HasMaxLength(200);
        builder.Property(p => p.LastName).HasMaxLength(200);
        builder.Property(p => p.FirstName).HasMaxLength(200);
        builder.Property(p => p.LastNameHash).HasMaxLength(64);
        builder.Property(p => p.FirstNameHash).HasMaxLength(64);
        builder.Property(p => p.DateOfBirthHash).HasMaxLength(64);
        
        // PatientIdentifier → Organization relationship
        builder.HasOne(p => p.SourceOrganization)
               .WithMany(o => o.PatientIdentifiers)
               .HasForeignKey(p => p.SourceOrganizationId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}