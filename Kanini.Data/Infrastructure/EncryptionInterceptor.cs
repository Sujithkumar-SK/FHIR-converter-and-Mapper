using Kanini.Common.Attributes;
using Kanini.Common.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Reflection;

namespace Kanini.Data.Infrastructure;

public class EncryptionInterceptor : SaveChangesInterceptor
{
    private readonly IEncryptionService _encryptionService;

    public EncryptionInterceptor(IEncryptionService encryptionService)
    {
        _encryptionService = encryptionService;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ProcessEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ProcessEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ProcessEntities(DbContext? context)
    {
        if (context == null) return;

        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            var entityType = entry.Entity.GetType();
            var properties = entityType.GetProperties()
                .Where(p => p.GetCustomAttribute<EncryptedAttribute>() != null);

            foreach (var property in properties)
            {
                var value = property.GetValue(entry.Entity) as string;
                if (!string.IsNullOrEmpty(value))
                {
                    var encryptedValue = _encryptionService.Encrypt(value);
                    property.SetValue(entry.Entity, encryptedValue);

                    // Handle hash fields for PatientIdentifier
                    if (entityType.Name == "PatientIdentifier")
                    {
                        HandlePatientIdentifierHashing(entry.Entity, property.Name, value);
                    }
                }
            }
        }
    }

    private void HandlePatientIdentifierHashing(object entity, string propertyName, string value)
    {
        var entityType = entity.GetType();
        
        switch (propertyName)
        {
            case "NationalId":
                var nationalIdHashProp = entityType.GetProperty("NationalIdHash");
                nationalIdHashProp?.SetValue(entity, _encryptionService.Hash(value));
                break;
            case "LastName":
                var lastNameHashProp = entityType.GetProperty("LastNameHash");
                lastNameHashProp?.SetValue(entity, _encryptionService.Hash(value));
                break;
            case "FirstName":
                var firstNameHashProp = entityType.GetProperty("FirstNameHash");
                firstNameHashProp?.SetValue(entity, _encryptionService.Hash(value));
                break;
        }

        // Handle DateOfBirth hashing
        var dobProp = entityType.GetProperty("DateOfBirth");
        var dobValue = dobProp?.GetValue(entity) as DateTime?;
        if (dobValue.HasValue)
        {
            var dobHashProp = entityType.GetProperty("DateOfBirthHash");
            dobHashProp?.SetValue(entity, _encryptionService.Hash(dobValue.Value.ToString("yyyy-MM-dd")));
        }
    }
}