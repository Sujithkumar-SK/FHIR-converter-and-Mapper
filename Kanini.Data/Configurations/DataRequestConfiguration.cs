using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Kanini.Domain.Entities;

namespace Kanini.Data.Configurations;

public class DataRequestConfiguration : IEntityTypeConfiguration<DataRequest>
{
    public void Configure(EntityTypeBuilder<DataRequest> builder)
    {
        builder.ToTable("DataRequests");

        builder.HasKey(dr => dr.RequestId);

        builder.Property(dr => dr.RequestId)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(dr => dr.GlobalPatientId)
            .IsRequired();

        builder.Property(dr => dr.RequestingUserId)
            .IsRequired();

        builder.Property(dr => dr.RequestingOrganizationId)
            .IsRequired();

        builder.Property(dr => dr.SourceOrganizationId)
            .IsRequired();

        builder.Property(dr => dr.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(dr => dr.Notes)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(dr => dr.ApprovedAt)
            .IsRequired(false);

        builder.Property(dr => dr.ApprovedByUserId)
            .IsRequired(false);

        builder.Property(dr => dr.ExpiresAt)
            .IsRequired();

        // Relationships
        builder.HasOne(dr => dr.RequestingUser)
            .WithMany(u => u.RequestedDataRequests)
            .HasForeignKey(dr => dr.RequestingUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(dr => dr.ApprovedByUser)
            .WithMany(u => u.ApprovedDataRequests)
            .HasForeignKey(dr => dr.ApprovedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(dr => dr.RequestingOrganization)
            .WithMany()
            .HasForeignKey(dr => dr.RequestingOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(dr => dr.SourceOrganization)
            .WithMany()
            .HasForeignKey(dr => dr.SourceOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(dr => new { dr.GlobalPatientId, dr.RequestingOrganizationId, dr.SourceOrganizationId })
            .HasDatabaseName("IX_DataRequests_Patient_Organizations");

        builder.HasIndex(dr => dr.Status)
            .HasDatabaseName("IX_DataRequests_Status");

        builder.HasIndex(dr => dr.ExpiresAt)
            .HasDatabaseName("IX_DataRequests_ExpiresAt");
    }
}