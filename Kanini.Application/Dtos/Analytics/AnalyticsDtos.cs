namespace Kanini.Application.DTOs.Analytics;

public class SystemOverviewDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalOrganizations { get; set; }
    public int TotalConversions { get; set; }
    public int TotalDataRequests { get; set; }
    public int PendingDataRequests { get; set; }
    public int TotalPatients { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class ConversionStatisticsDto
{
    public int TotalConversions { get; set; }
    public int SuccessfulConversions { get; set; }
    public int FailedConversions { get; set; }
    public int ProcessingConversions { get; set; }
    public decimal SuccessRate { get; set; }
    public long AverageProcessingTimeMs { get; set; }
    public List<ConversionByFormatDto> ConversionsByFormat { get; set; } = new();
    public List<ConversionTrendDto> ConversionTrends { get; set; } = new();
}

public class UserActivityStatsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public List<UsersByRoleDto> UsersByRole { get; set; } = new();
    public List<UserActivityDto> RecentActivity { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class DataRequestStatsDto
{
    public int TotalRequests { get; set; }
    public int PendingRequests { get; set; }
    public int ApprovedRequests { get; set; }
    public int RejectedRequests { get; set; }
    public int CompletedRequests { get; set; }
    public int ExpiredRequests { get; set; }
    public decimal ApprovalRate { get; set; }
    public double AverageProcessingHours { get; set; }
    public List<RequestsByStatusDto> RequestsByStatus { get; set; } = new();
    public List<RequestTrendDto> RequestTrends { get; set; } = new();
}

public class OrganizationStatsDto
{
    public int TotalOrganizations { get; set; }
    public int ActiveOrganizations { get; set; }
    public int HospitalCount { get; set; }
    public int ClinicCount { get; set; }
    public List<OrganizationActivityDto> TopActiveOrganizations { get; set; } = new();
    public List<OrganizationByTypeDto> OrganizationsByType { get; set; } = new();
}