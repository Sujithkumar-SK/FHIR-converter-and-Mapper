using Microsoft.Data.SqlClient;
using Kanini.Data.Infrastructure;
using Microsoft.Extensions.Logging;
using System.Data;
using Microsoft.Extensions.Configuration;
using Kanini.Domain.Entities;
using Kanini.Domain.Enums;
using Kanini.Domain.Analytics;

namespace Kanini.Data.Infrastructure;

public class DatabaseReader : IDatabaseReader
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseReader> _logger;

    public DatabaseReader(IConfiguration configuration, ILogger<DatabaseReader> logger)
    {
        _connectionString = configuration.GetConnectionString("DatabaseConnectionString")!;
        _logger = logger;
    }

    public async Task<T?> QuerySingleOrDefaultAsync<T>(string storedProcedure, object? parameters = null)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(storedProcedure, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (parameters != null)
            {
                AddParameters(command, parameters);
            }

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return MapToObject<T>(reader);
            }
            
            return default(T);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing stored procedure: {StoredProcedure}", storedProcedure);
            throw;
        }
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string storedProcedure, object? parameters = null)
    {
        try
        {
            var results = new List<T>();
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(storedProcedure, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (parameters != null)
            {
                AddParameters(command, parameters);
            }

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                var item = MapToObject<T>(reader);
                if (item != null)
                    results.Add(item);
            }
            
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing stored procedure: {StoredProcedure}", storedProcedure);
            throw;
        }
    }

    private void AddParameters(SqlCommand command, object parameters)
    {
        var properties = parameters.GetType().GetProperties();
        foreach (var property in properties)
        {
            var value = property.GetValue(parameters) ?? DBNull.Value;
            command.Parameters.AddWithValue($"@{property.Name}", value);
        }
    }

    private T? MapToObject<T>(SqlDataReader reader)
    {
        if (typeof(T) == typeof(bool))
        {
            return (T)(object)(reader.GetBoolean("UserExists"));
        }
        
        if (typeof(T) == typeof(User))
        {
            var user = new User
            {
                UserId = reader.GetGuid("UserId"),
                Email = reader.GetString("Email"),
                PasswordHash = reader.GetString("PasswordHash"),
                Role = (UserRole)reader.GetInt32("Role"),
                OrganizationId = reader.IsDBNull("OrganizationId") ? null : reader.GetGuid("OrganizationId"),
                IsActive = reader.GetBoolean("IsActive"),
                LastLogin = reader.IsDBNull("LastLogin") ? null : reader.GetDateTime("LastLogin"),
                CreatedBy = reader.IsDBNull("CreatedBy") ? string.Empty : reader.GetString("CreatedBy"),
                CreatedOn = reader.GetDateTime("CreatedOn"),
                UpdatedBy = reader.IsDBNull("UpdatedBy") ? null : reader.GetString("UpdatedBy"),
                UpdatedOn = reader.IsDBNull("UpdatedOn") ? null : reader.GetDateTime("UpdatedOn")
            };
            
            // Set organization if available
            if (!reader.IsDBNull("OrganizationName"))
            {
                user.Organization = new Organization
                {
                    Name = reader.GetString("OrganizationName")
                };
            }
            
            return (T)(object)user;
        }
        
        if (typeof(T) == typeof(PatientIdentifier))
        {
            var patient = new PatientIdentifier
            {
                Id = reader.GetGuid("Id"),
                GlobalPatientId = reader.GetGuid("GlobalPatientId"),
                SourceOrganizationId = reader.GetGuid("SourceOrganizationId"),
                LocalPatientId = reader.GetString("LocalPatientId"),
                LastName = reader.IsDBNull("LastName") ? null : reader.GetString("LastName"),
                FirstName = reader.IsDBNull("FirstName") ? null : reader.GetString("FirstName"),
                DateOfBirth = reader.IsDBNull("DateOfBirth") ? null : reader.GetDateTime("DateOfBirth"),
                LastNameHash = reader.IsDBNull("LastNameHash") ? null : reader.GetString("LastNameHash"),
                FirstNameHash = reader.IsDBNull("FirstNameHash") ? null : reader.GetString("FirstNameHash"),
                DateOfBirthHash = reader.IsDBNull("DateOfBirthHash") ? null : reader.GetString("DateOfBirthHash"),
                CreatedBy = reader.IsDBNull("CreatedBy") ? string.Empty : reader.GetString("CreatedBy"),
                CreatedOn = reader.GetDateTime("CreatedOn"),
                UpdatedBy = reader.IsDBNull("UpdatedBy") ? null : reader.GetString("UpdatedBy"),
                UpdatedOn = reader.IsDBNull("UpdatedOn") ? null : reader.GetDateTime("UpdatedOn")
            };
            
            // Set organization if available
            if (!reader.IsDBNull("OrganizationName"))
            {
                patient.SourceOrganization = new Organization
                {
                    Name = reader.GetString("OrganizationName")
                };
            }
            return (T)(object)patient;
        }
        
        if (typeof(T) == typeof(DataRequest))
        {
            var dataRequest = new DataRequest
            {
                RequestId = reader.GetGuid("RequestId"),
                GlobalPatientId = reader.GetGuid("GlobalPatientId"),
                RequestingUserId = reader.GetGuid("RequestingUserId"),
                RequestingOrganizationId = reader.GetGuid("RequestingOrganizationId"),
                SourceOrganizationId = reader.GetGuid("SourceOrganizationId"),
                Status = (DataRequestStatus)reader.GetInt32("Status"),
                Notes = reader.IsDBNull("Notes") ? null : reader.GetString("Notes"),
                ApprovedAt = reader.IsDBNull("ApprovedAt") ? null : reader.GetDateTime("ApprovedAt"),
                ApprovedByUserId = reader.IsDBNull("ApprovedByUserId") ? null : reader.GetGuid("ApprovedByUserId"),
                ExpiresAt = reader.GetDateTime("ExpiresAt"),
                CreatedBy = reader.IsDBNull("CreatedBy") ? string.Empty : reader.GetString("CreatedBy"),
                CreatedOn = reader.GetDateTime("CreatedOn"),
                UpdatedBy = reader.IsDBNull("UpdatedBy") ? null : reader.GetString("UpdatedBy"),
                UpdatedOn = reader.IsDBNull("UpdatedOn") ? null : reader.GetDateTime("UpdatedOn")
            };
            
            // Set organizations if available
            if (!reader.IsDBNull("RequestingOrganizationName"))
            {
                dataRequest.RequestingOrganization = new Organization
                {
                    OrganizationId = reader.GetGuid("RequestingOrganizationId"),
                    Name = reader.GetString("RequestingOrganizationName")
                };
            }
            
            if (!reader.IsDBNull("SourceOrganizationName"))
            {
                dataRequest.SourceOrganization = new Organization
                {
                    OrganizationId = reader.GetGuid("SourceOrganizationId"),
                    Name = reader.GetString("SourceOrganizationName")
                };
            }
            
            // Set users if available
            if (!reader.IsDBNull("RequestingUserEmail"))
            {
                dataRequest.RequestingUser = new User
                {
                    UserId = reader.GetGuid("RequestingUserId"),
                    Email = reader.GetString("RequestingUserEmail")
                };
            }
            
            if (!reader.IsDBNull("ApprovedByUserEmail"))
            {
                dataRequest.ApprovedByUser = new User
                {
                    UserId = reader.GetGuid("ApprovedByUserId"),
                    Email = reader.GetString("ApprovedByUserEmail")
                };
            }
            
            return (T)(object)dataRequest;
        }
        
        // Analytics models mapping
        if (typeof(T) == typeof(SystemOverview))
        {
            var overview = new SystemOverview
            {
                TotalUsers = reader.GetInt32("TotalUsers"),
                ActiveUsers = reader.GetInt32("ActiveUsers"),
                TotalOrganizations = reader.GetInt32("TotalOrganizations"),
                TotalConversions = reader.GetInt32("TotalConversions"),
                TotalDataRequests = reader.GetInt32("TotalDataRequests"),
                PendingDataRequests = reader.GetInt32("PendingDataRequests"),
                TotalPatients = reader.GetInt32("TotalPatients"),
                LastUpdated = reader.GetDateTime("LastUpdated")
            };
            return (T)(object)overview;
        }
        
        if (typeof(T) == typeof(ConversionStatistics))
        {
            var stats = new ConversionStatistics
            {
                TotalConversions = reader.GetInt32("TotalConversions"),
                SuccessfulConversions = reader.GetInt32("SuccessfulConversions"),
                FailedConversions = reader.GetInt32("FailedConversions"),
                ProcessingConversions = reader.GetInt32("ProcessingConversions"),
                SuccessRate = reader.GetDecimal("SuccessRate"),
                AverageProcessingTimeMs = reader.GetInt64("AverageProcessingTimeMs")
            };
            return (T)(object)stats;
        }
        
        if (typeof(T) == typeof(UserActivityStats))
        {
            var stats = new UserActivityStats
            {
                TotalUsers = reader.GetInt32("TotalUsers"),
                ActiveUsers = reader.GetInt32("ActiveUsers"),
                InactiveUsers = reader.GetInt32("InactiveUsers"),
                LastUpdated = reader.GetDateTime("LastUpdated")
            };
            return (T)(object)stats;
        }
        
        if (typeof(T) == typeof(DataRequestStats))
        {
            var stats = new DataRequestStats
            {
                TotalRequests = reader.GetInt32("TotalRequests"),
                PendingRequests = reader.GetInt32("PendingRequests"),
                ApprovedRequests = reader.GetInt32("ApprovedRequests"),
                RejectedRequests = reader.GetInt32("RejectedRequests"),
                CompletedRequests = reader.GetInt32("CompletedRequests"),
                ExpiredRequests = reader.GetInt32("ExpiredRequests"),
                ApprovalRate = reader.GetDecimal("ApprovalRate"),
                AverageProcessingHours = reader.GetDouble("AverageProcessingHours")
            };
            return (T)(object)stats;
        }
        
        if (typeof(T) == typeof(OrganizationStats))
        {
            var stats = new OrganizationStats
            {
                TotalOrganizations = reader.GetInt32("TotalOrganizations"),
                ActiveOrganizations = reader.GetInt32("ActiveOrganizations"),
                HospitalCount = reader.GetInt32("HospitalCount"),
                ClinicCount = reader.GetInt32("ClinicCount")
            };
            return (T)(object)stats;
        }
        
        return default(T);
    }
}