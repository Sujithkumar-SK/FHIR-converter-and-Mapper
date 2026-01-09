using AutoMapper;
using Kanini.Application.DTOs.DataRequests;
using Kanini.Application.Services.DataRequests;
using Kanini.Common.Constants;
using Kanini.Common.Results;
using Kanini.Data.Repositories.DataRequests;
using Kanini.Domain.Entities;
using Kanini.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Kanini.Application.Services.DataRequests;

public class DataRequestService : IDataRequestService
{
    private readonly IDataRequestRepository _dataRequestRepository;
    private readonly IDataRequestReadRepository _dataRequestReadRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<DataRequestService> _logger;

    public DataRequestService(
        IDataRequestRepository dataRequestRepository,
        IDataRequestReadRepository dataRequestReadRepository,
        IMapper mapper,
        ILogger<DataRequestService> logger)
    {
        _dataRequestRepository = dataRequestRepository;
        _dataRequestReadRepository = dataRequestReadRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<DataRequestResponseDto>> CreateRequestAsync(CreateDataRequestDto request, Guid requestingUserId, Guid requestingOrganizationId, string createdBy)
    {
        try
        {
            _logger.LogInformation(MagicStrings.LogMessages.DataRequestCreationStarted, request.GlobalPatientId);

            // Validate that the requesting organization is not the same as source organization
            if (requestingOrganizationId == request.SourceOrganizationId)
            {
                return Result.Failure<DataRequestResponseDto>("Cannot request data for patients from your own organization");
            }

            // Check if request already exists
            var existingRequest = await _dataRequestReadRepository.CheckRequestExistsAsync(
                request.GlobalPatientId, requestingOrganizationId, request.SourceOrganizationId);

            if (existingRequest)
            {
                return Result.Failure<DataRequestResponseDto>(MagicStrings.ErrorMessages.DataRequestAlreadyExists);
            }

            // Create new data request
            var dataRequest = _mapper.Map<DataRequest>(request);
            dataRequest.RequestingUserId = requestingUserId;
            dataRequest.RequestingOrganizationId = requestingOrganizationId;
            dataRequest.CreatedBy = createdBy;
            dataRequest.CreatedOn = DateTime.UtcNow;

            var createdRequest = await _dataRequestRepository.CreateAsync(dataRequest);

            // Get full request details for response
            var fullRequest = await _dataRequestReadRepository.GetByIdAsync(createdRequest.RequestId);
            if (fullRequest == null)
            {
                return Result.Failure<DataRequestResponseDto>(MagicStrings.ErrorMessages.DataRequestCreationFailed);
            }

            var responseDto = _mapper.Map<DataRequestResponseDto>(fullRequest);

            _logger.LogInformation(MagicStrings.LogMessages.DataRequestCreationCompleted, createdRequest.RequestId);
            return Result.Success(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, MagicStrings.LogMessages.DataRequestCreationFailed, request.GlobalPatientId, ex.Message);
            return Result.Failure<DataRequestResponseDto>(MagicStrings.ErrorMessages.DataRequestCreationFailed);
        }
    }

    public async Task<Result<IEnumerable<DataRequestResponseDto>>> GetRequestsByOrganizationAsync(Guid organizationId, bool isRequesting = true)
    {
        try
        {
            var requests = await _dataRequestReadRepository.GetByOrganizationAsync(organizationId, isRequesting);
            var responseDtos = _mapper.Map<IEnumerable<DataRequestResponseDto>>(requests);
            return Result.Success(responseDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting data requests for organization {OrganizationId}", organizationId);
            return Result.Failure<IEnumerable<DataRequestResponseDto>>(MagicStrings.ErrorMessages.InternalServerError);
        }
    }

    public async Task<Result<DataRequestResponseDto>> GetRequestByIdAsync(Guid requestId)
    {
        try
        {
            var request = await _dataRequestReadRepository.GetByIdAsync(requestId);
            if (request == null)
            {
                return Result.Failure<DataRequestResponseDto>(MagicStrings.ErrorMessages.DataRequestNotFound);
            }

            var responseDto = _mapper.Map<DataRequestResponseDto>(request);
            return Result.Success(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting data request {RequestId}", requestId);
            return Result.Failure<DataRequestResponseDto>(MagicStrings.ErrorMessages.InternalServerError);
        }
    }

    public async Task<Result<DataRequestResponseDto>> ApproveRequestAsync(Guid requestId, ApproveDataRequestDto approval, Guid approvedByUserId, string updatedBy)
    {
        try
        {
            _logger.LogInformation(MagicStrings.LogMessages.DataRequestApprovalStarted, requestId);

            var request = await _dataRequestReadRepository.GetByIdAsync(requestId);
            if (request == null)
            {
                return Result.Failure<DataRequestResponseDto>(MagicStrings.ErrorMessages.DataRequestNotFound);
            }

            if (request.Status != DataRequestStatus.Pending)
            {
                return Result.Failure<DataRequestResponseDto>(MagicStrings.ErrorMessages.InvalidDataRequestStatus);
            }

            if (request.ExpiresAt <= DateTime.UtcNow)
            {
                return Result.Failure<DataRequestResponseDto>(MagicStrings.ErrorMessages.DataRequestExpired);
            }

            // Update request
            request.Status = approval.Status;
            request.ApprovedAt = DateTime.UtcNow;
            request.ApprovedByUserId = approvedByUserId;
            request.UpdatedBy = updatedBy;
            request.UpdatedOn = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(approval.Notes))
            {
                request.Notes = approval.Notes;
            }

            var updatedRequest = await _dataRequestRepository.UpdateAsync(request);

            // Get full updated request details
            var fullRequest = await _dataRequestReadRepository.GetByIdAsync(updatedRequest.RequestId);
            var responseDto = _mapper.Map<DataRequestResponseDto>(fullRequest!);

            _logger.LogInformation(MagicStrings.LogMessages.DataRequestApprovalCompleted, requestId);
            return Result.Success(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, MagicStrings.LogMessages.DataRequestApprovalFailed, requestId, ex.Message);
            return Result.Failure<DataRequestResponseDto>(MagicStrings.ErrorMessages.DataRequestApprovalFailed);
        }
    }

    public async Task<Result<IEnumerable<DataRequestResponseDto>>> GetPendingRequestsAsync(Guid sourceOrganizationId)
    {
        try
        {
            var requests = await _dataRequestReadRepository.GetPendingRequestsAsync(sourceOrganizationId);
            var responseDtos = _mapper.Map<IEnumerable<DataRequestResponseDto>>(requests);
            return Result.Success(responseDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending requests for organization {OrganizationId}", sourceOrganizationId);
            return Result.Failure<IEnumerable<DataRequestResponseDto>>(MagicStrings.ErrorMessages.InternalServerError);
        }
    }
}