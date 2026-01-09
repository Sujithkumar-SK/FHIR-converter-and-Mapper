using Kanini.Domain.Entities;

namespace Kanini.Data.Repositories.DataRequests;

public interface IDataRequestReadRepository
{
    Task<IEnumerable<DataRequest>> GetByOrganizationAsync(Guid organizationId, bool isRequesting = true);
    Task<DataRequest?> GetByIdAsync(Guid requestId);
    Task<IEnumerable<DataRequest>> GetPendingRequestsAsync(Guid sourceOrganizationId);
    Task<bool> CheckRequestExistsAsync(Guid globalPatientId, Guid requestingOrganizationId, Guid sourceOrganizationId);
}