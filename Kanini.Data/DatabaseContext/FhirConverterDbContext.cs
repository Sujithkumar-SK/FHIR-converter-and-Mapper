using Kanini.Domain.Entities;
using Kanini.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Kanini.Data.DatabaseContext;

public class FhirConverterDbContext(DbContextOptions<FhirConverterDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<PatientIdentifier> PatientIdentifiers { get; set; }
    public DbSet<DataRequest> DataRequests { get; set; }
    public DbSet<ConversionJob> ConversionJobs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FhirConverterDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
