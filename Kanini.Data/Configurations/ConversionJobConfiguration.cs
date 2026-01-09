using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Kanini.Domain.Entities;

namespace Kanini.Data.Configurations;

public class ConversionJobConfiguration : IEntityTypeConfiguration<ConversionJob>
{
    public void Configure(EntityTypeBuilder<ConversionJob> builder)
    {
        builder.HasKey(c => c.JobId);
        
        // Configure relationships
        builder.HasOne(c => c.User)
               .WithMany(u => u.ConversionJobs)
               .HasForeignKey(c => c.UserId)
               .OnDelete(DeleteBehavior.Restrict);
               
        builder.HasOne(c => c.DataRequest)
               .WithMany(d => d.ConversionJobs)
               .HasForeignKey(c => c.RequestId)
               .OnDelete(DeleteBehavior.Restrict);
               
        builder.Property(c => c.ErrorMessage).HasMaxLength(2000);
        builder.Property(c => c.OriginalFileName).HasMaxLength(200);
    }
}