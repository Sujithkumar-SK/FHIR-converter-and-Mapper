using Kanini.Domain.Entities;

namespace Kanini.Data.Repositories.DataRequests;

public interface IDataRequestRepository
{
    Task<DataRequest> CreateAsync(DataRequest dataRequest);
    Task<DataRequest> UpdateAsync(DataRequest dataRequest);
    Task DeleteAsync(Guid requestId);
}