using Kanini.Data.Infrastructure;
using Kanini.Domain.Entities;
using Kanini.Common.Constants;
using Microsoft.Extensions.Logging;

namespace Kanini.Data.Repositories.DataRequests;

public class DataRequestReadRepository : IDataRequestReadRepository
{
    private readonly IDatabaseReader _databaseReader;
    private readonly ILogger<DataRequestReadRepository> _logger;

    public DataRequestReadRepository(IDatabaseReader databaseReader, ILogger<DataRequestReadRepository> logger)
    {
        _databaseReader = databaseReader;
        _logger = logger;
    }

    public async Task<IEnumerable<DataRequest>> GetByOrganizationAsync(Guid organizationId, bool isRequesting = true)
    {
        try
        {
            return await _databaseReader.QueryAsync<DataRequest>(
                MagicStrings.StoredProcedures.GetDataRequestsByOrganization,
                new { OrganizationId = organizationId, IsRequesting = isRequesting }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting data requests for organization {OrganizationId}", organizationId);
            throw;
        }
    }

    public async Task<DataRequest?> GetByIdAsync(Guid requestId)
    {
        try
        {
            return await _databaseReader.QuerySingleOrDefaultAsync<DataRequest>(
                MagicStrings.StoredProcedures.GetDataRequestById,
                new { RequestId = requestId }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting data request {RequestId}", requestId);
            throw;
        }
    }

    public async Task<IEnumerable<DataRequest>> GetPendingRequestsAsync(Guid sourceOrganizationId)
    {
        try
        {
            return await _databaseReader.QueryAsync<DataRequest>(
                MagicStrings.StoredProcedures.GetPendingDataRequests,
                new { SourceOrganizationId = sourceOrganizationId }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending requests for organization {OrganizationId}", sourceOrganizationId);
            throw;
        }
    }

    public async Task<bool> CheckRequestExistsAsync(Guid globalPatientId, Guid requestingOrganizationId, Guid sourceOrganizationId)
    {
        try
        {
            return await _databaseReader.QuerySingleOrDefaultAsync<bool>(
                MagicStrings.StoredProcedures.CheckDataRequestExists,
                new { 
                    GlobalPatientId = globalPatientId,
                    RequestingOrganizationId = requestingOrganizationId,
                    SourceOrganizationId = sourceOrganizationId
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if data request exists for patient {GlobalPatientId}", globalPatientId);
            throw;
        }
    }
}