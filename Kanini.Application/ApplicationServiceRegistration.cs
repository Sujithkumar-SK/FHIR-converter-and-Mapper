using Kanini.Application.Services.Users;
using Kanini.Application.Services.Patients;
using Kanini.Application.Services.DataRequests;
using Kanini.Application.Services.Files;
using Kanini.Application.Services.Conversion;
using Kanini.Application.Services.Analytics;
using Kanini.Application.Fhir;
using Kanini.Application.Parsers;
using Microsoft.Extensions.DependencyInjection;

namespace Kanini.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // AutoMapper
        services.AddAutoMapper(typeof(Kanini.Application.AutoMapper.UserMappingProfile));
        services.AddAutoMapper(typeof(Kanini.Application.AutoMapper.PatientMappingProfile));
        services.AddAutoMapper(typeof(Kanini.Application.AutoMapper.DataRequestMappingProfile));
        services.AddAutoMapper(typeof(Kanini.Application.AutoMapper.FileMappingProfile));
        services.AddAutoMapper(typeof(Kanini.Application.AutoMapper.ConversionMappingProfile));
        services.AddAutoMapper(typeof(Kanini.Application.AutoMapper.AnalyticsMappingProfile));

        // Application Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IOTPService, OTPService>();
        services.AddScoped<IJwtService, JwtService>();
        
        // Patient Services
        services.AddScoped<IPatientService, PatientService>();
        
        // Data Request Services
        services.AddScoped<IDataRequestService, DataRequestService>();
        
        // File Services
        services.AddScoped<IFileUploadService, FileUploadService>();
        services.AddScoped<IFileValidationService, FileValidationService>();
        services.AddScoped<IFileCleanupService, FileCleanupService>();
        services.AddSingleton<ITempFileManager, TempFileManager>();
        
        // Conversion Services
        services.AddScoped<IFhirConversionService, FhirConversionService>();
        services.AddScoped<IFieldDetectionService, FieldDetectionService>();
        
        // FHIR Services
        services.AddScoped<IFhirConverter, FhirConverter>();
        
        // Parser Services
        services.AddScoped<ICsvParser, CsvParser>();
        services.AddScoped<IJsonParser, JsonParser>();
        services.AddScoped<ICcdaParser, CcdaParser>();
        
        // Analytics Services
        services.AddScoped<IAnalyticsService, AnalyticsService>();

        return services;
    }
}