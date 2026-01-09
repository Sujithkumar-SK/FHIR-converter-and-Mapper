using Kanini.Domain.Enums;

namespace Kanini.Application.DTOs.Analytics;

public class ConversionByFormatDto
{
    public InputFormat Format { get; set; }
    public string FormatName { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public class ConversionTrendDto
{
    public DateTime Date { get; set; }
    public int ConversionCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
}

public class UsersByRoleDto
{
    public UserRole Role { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public class UserActivityDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public DateTime LastLogin { get; set; }
    public int ConversionsCount { get; set; }
    public int DataRequestsCount { get; set; }
}

public class RequestsByStatusDto
{
    public DataRequestStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public class RequestTrendDto
{
    public DateTime Date { get; set; }
    public int RequestCount { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
}

public class OrganizationActivityDto
{
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public OrganizationType Type { get; set; }
    public int UserCount { get; set; }
    public int ConversionCount { get; set; }
    public int DataRequestCount { get; set; }
    public DateTime LastActivity { get; set; }
}

public class OrganizationByTypeDto
{
    public OrganizationType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}