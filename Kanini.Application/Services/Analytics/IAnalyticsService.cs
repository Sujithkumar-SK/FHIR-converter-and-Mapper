using Kanini.Application.DTOs.Analytics;
using Kanini.Common.Results;

namespace Kanini.Application.Services.Analytics;

public interface IAnalyticsService
{
    Task<Result<SystemOverviewDto>> GetSystemOverviewAsync();
    Task<Result<ConversionStatisticsDto>> GetConversionStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<Result<UserActivityStatsDto>> GetUserActivityStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<Result<DataRequestStatsDto>> GetDataRequestStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<Result<OrganizationStatsDto>> GetOrganizationStatsAsync();
}