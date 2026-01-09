using Kanini.Common.Attributes;
using Kanini.Common.Services;
using System.Reflection;

namespace Kanini.Data.Infrastructure;

public static class DecryptionExtensions
{
    public static T DecryptEntity<T>(this T entity, IEncryptionService encryptionService) where T : class
    {
        try
        {
            var entityType = typeof(T);
            var properties = entityType.GetProperties()
                .Where(p => p.GetCustomAttribute<EncryptedAttribute>() != null);

            foreach (var property in properties)
            {
                var encryptedValue = property.GetValue(entity) as string;
                if (!string.IsNullOrEmpty(encryptedValue))
                {
                    var decryptedValue = encryptionService.Decrypt(encryptedValue);
                    property.SetValue(entity, decryptedValue);
                }
            }

            return entity;
        }
        catch
        {
            return entity;
        }
    }

    public static IEnumerable<T> DecryptEntities<T>(this IEnumerable<T> entities, IEncryptionService encryptionService) where T : class
    {
        return entities.Select(entity => entity.DecryptEntity(encryptionService));
    }
}