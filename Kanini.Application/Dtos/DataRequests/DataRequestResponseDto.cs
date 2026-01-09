using Kanini.Domain.Enums;

namespace Kanini.Application.DTOs.DataRequests;

public class DataRequestResponseDto
{
    public Guid RequestId { get; set; }
    public Guid GlobalPatientId { get; set; }
    public Guid RequestingUserId { get; set; }
    public Guid RequestingOrganizationId { get; set; }
    public Guid SourceOrganizationId { get; set; }
    public DataRequestStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedOn { get; set; }
    
    // Organization details
    public string RequestingOrganizationName { get; set; } = null!;
    public string SourceOrganizationName { get; set; } = null!;
    
    // User details
    public string RequestingUserEmail { get; set; } = null!;
    public string? ApprovedByUserEmail { get; set; }
}