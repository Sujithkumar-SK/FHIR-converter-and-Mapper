using Kanini.Data.DatabaseContext;
using Kanini.Data.Infrastructure;
using Kanini.Data.Repositories.Users;
using Kanini.Data.Repositories.Organizations;
using Kanini.Data.Repositories.Patients;
using Kanini.Data.Repositories.DataRequests;
using Kanini.Common.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kanini.Data;

public static class DataServiceRegistration
{
    public static IServiceCollection AddDataServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Encryption Service
        services.AddSingleton<IEncryptionService, EncryptionService>();

        // Database Context with Encryption Interceptor
        services.AddDbContext<FhirConverterDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DatabaseConnectionString"));
        });

        // Configure interceptor separately
        services.AddScoped<EncryptionInterceptor>();

        // ADO.NET Infrastructure
        services.AddScoped<IDatabaseReader>(provider =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var logger = provider.GetRequiredService<ILogger<DatabaseReader>>();
            return new DatabaseReader(configuration, logger);
        });

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserReadRepository, UserReadRepository>();
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        
        // Patient Repositories
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IPatientReadRepository, PatientReadRepository>();
        
        // Data Request Repositories
        services.AddScoped<IDataRequestRepository, DataRequestRepository>();
        services.AddScoped<IDataRequestReadRepository, DataRequestReadRepository>();

        return services;
    }
}