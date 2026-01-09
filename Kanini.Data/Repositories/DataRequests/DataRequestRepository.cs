using Kanini.Data.DatabaseContext;
using Kanini.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Kanini.Data.Repositories.DataRequests;

public class DataRequestRepository : IDataRequestRepository
{
    private readonly FhirConverterDbContext _context;
    private readonly ILogger<DataRequestRepository> _logger;

    public DataRequestRepository(FhirConverterDbContext context, ILogger<DataRequestRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DataRequest> CreateAsync(DataRequest dataRequest)
    {
        try
        {
            _context.DataRequests.Add(dataRequest);
            await _context.SaveChangesAsync();
            return dataRequest;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating data request for patient {GlobalPatientId}", dataRequest.GlobalPatientId);
            throw;
        }
    }

    public async Task<DataRequest> UpdateAsync(DataRequest dataRequest)
    {
        try
        {
            // Attach the entity and mark only specific properties as modified
            var existingEntity = await _context.DataRequests.FindAsync(dataRequest.RequestId);
            if (existingEntity == null)
            {
                throw new InvalidOperationException($"DataRequest with ID {dataRequest.RequestId} not found");
            }

            // Update only the properties we want to change
            existingEntity.Status = dataRequest.Status;
            existingEntity.ApprovedAt = dataRequest.ApprovedAt;
            existingEntity.ApprovedByUserId = dataRequest.ApprovedByUserId;
            existingEntity.Notes = dataRequest.Notes;
            existingEntity.UpdatedBy = dataRequest.UpdatedBy;
            existingEntity.UpdatedOn = dataRequest.UpdatedOn;

            await _context.SaveChangesAsync();
            return existingEntity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating data request {RequestId}", dataRequest.RequestId);
            throw;
        }
    }

    public async Task DeleteAsync(Guid requestId)
    {
        try
        {
            var dataRequest = await _context.DataRequests.FindAsync(requestId);
            if (dataRequest != null)
            {
                _context.DataRequests.Remove(dataRequest);
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting data request {RequestId}", requestId);
            throw;
        }
    }
}