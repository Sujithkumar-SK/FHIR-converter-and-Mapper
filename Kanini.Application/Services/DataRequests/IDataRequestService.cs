using Kanini.Application.DTOs.DataRequests;
using Kanini.Common.Results;

namespace Kanini.Application.Services.DataRequests;

public interface IDataRequestService
{
    Task<Result<DataRequestResponseDto>> CreateRequestAsync(CreateDataRequestDto request, Guid requestingUserId, Guid requestingOrganizationId, string createdBy);
    Task<Result<IEnumerable<DataRequestResponseDto>>> GetRequestsByOrganizationAsync(Guid organizationId, bool isRequesting = true);
    Task<Result<DataRequestResponseDto>> GetRequestByIdAsync(Guid requestId);
    Task<Result<DataRequestResponseDto>> ApproveRequestAsync(Guid requestId, ApproveDataRequestDto approval, Guid approvedByUserId, string updatedBy);
    Task<Result<IEnumerable<DataRequestResponseDto>>> GetPendingRequestsAsync(Guid sourceOrganizationId);
}