using Kanini.Application.DTOs.Analytics;
using Kanini.Common.Results;
using Kanini.Common.Constants;
using Kanini.Data.Infrastructure;
using Kanini.Domain.Analytics;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace Kanini.Application.Services.Analytics;

public class AnalyticsService : IAnalyticsService
{
    private readonly IDatabaseReader _databaseReader;
    private readonly IMapper _mapper;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(IDatabaseReader databaseReader, IMapper mapper, ILogger<AnalyticsService> logger)
    {
        _databaseReader = databaseReader;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<SystemOverviewDto>> GetSystemOverviewAsync()
    {
        try
        {
            _logger.LogInformation(MagicStrings.LogMessages.AnalyticsRequestStarted, "SystemOverview");

            var result = await _databaseReader.QuerySingleOrDefaultAsync<SystemOverview>(
                MagicStrings.StoredProcedures.GetSystemOverview);

            var overview = result;
            if (overview == null)
            {
                return Result.Failure<SystemOverviewDto>(MagicStrings.ErrorMessages.AnalyticsDataNotFound);
            }

            var dto = _mapper.Map<SystemOverviewDto>(overview);
            _logger.LogInformation(MagicStrings.LogMessages.AnalyticsRequestCompleted, "SystemOverview", 1);
            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, MagicStrings.LogMessages.AnalyticsRequestFailed, "SystemOverview", ex.Message);
            return Result.Failure<SystemOverviewDto>(MagicStrings.ErrorMessages.InternalServerError);
        }
    }

    public async Task<Result<ConversionStatisticsDto>> GetConversionStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            _logger.LogInformation(MagicStrings.LogMessages.AnalyticsRequestStarted, "ConversionStatistics");

            var parameters = new
            {
                StartDate = startDate ?? DateTime.UtcNow.AddDays(-30),
                EndDate = endDate ?? DateTime.UtcNow
            };

            var result = await _databaseReader.QuerySingleOrDefaultAsync<ConversionStatistics>(
                MagicStrings.StoredProcedures.GetConversionStatistics, parameters);

            var stats = result;
            if (stats == null)
            {
                return Result.Failure<ConversionStatisticsDto>(MagicStrings.ErrorMessages.AnalyticsDataNotFound);
            }

            var dto = _mapper.Map<ConversionStatisticsDto>(stats);
            _logger.LogInformation(MagicStrings.LogMessages.AnalyticsRequestCompleted, "ConversionStatistics", 1);
            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, MagicStrings.LogMessages.AnalyticsRequestFailed, "ConversionStatistics", ex.Message);
            return Result.Failure<ConversionStatisticsDto>(MagicStrings.ErrorMessages.InternalServerError);
        }
    }

    public async Task<Result<UserActivityStatsDto>> GetUserActivityStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            _logger.LogInformation(MagicStrings.LogMessages.AnalyticsRequestStarted, "UserActivityStats");

            var parameters = new
            {
                StartDate = startDate ?? DateTime.UtcNow.AddDays(-30),
                EndDate = endDate ?? DateTime.UtcNow
            };

            var result = await _databaseReader.QuerySingleOrDefaultAsync<UserActivityStats>(
                MagicStrings.StoredProcedures.GetUserActivityStats, parameters);

            var stats = result;
            if (stats == null)
            {
                return Result.Failure<UserActivityStatsDto>(MagicStrings.ErrorMessages.AnalyticsDataNotFound);
            }

            var dto = _mapper.Map<UserActivityStatsDto>(stats);
            _logger.LogInformation(MagicStrings.LogMessages.AnalyticsRequestCompleted, "UserActivityStats", 1);
            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, MagicStrings.LogMessages.AnalyticsRequestFailed, "UserActivityStats", ex.Message);
            return Result.Failure<UserActivityStatsDto>(MagicStrings.ErrorMessages.InternalServerError);
        }
    }

    public async Task<Result<DataRequestStatsDto>> GetDataRequestStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            _logger.LogInformation(MagicStrings.LogMessages.AnalyticsRequestStarted, "DataRequestStats");

            var parameters = new
            {
                StartDate = startDate ?? DateTime.UtcNow.AddDays(-30),
                EndDate = endDate ?? DateTime.UtcNow
            };

            var result = await _databaseReader.QuerySingleOrDefaultAsync<DataRequestStats>(
                MagicStrings.StoredProcedures.GetDataRequestStats, parameters);

            var stats = result;
            if (stats == null)
            {
                return Result.Failure<DataRequestStatsDto>(MagicStrings.ErrorMessages.AnalyticsDataNotFound);
            }

            var dto = _mapper.Map<DataRequestStatsDto>(stats);
            _logger.LogInformation(MagicStrings.LogMessages.AnalyticsRequestCompleted, "DataRequestStats", 1);
            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, MagicStrings.LogMessages.AnalyticsRequestFailed, "DataRequestStats", ex.Message);
            return Result.Failure<DataRequestStatsDto>(MagicStrings.ErrorMessages.InternalServerError);
        }
    }

    public async Task<Result<OrganizationStatsDto>> GetOrganizationStatsAsync()
    {
        try
        {
            _logger.LogInformation(MagicStrings.LogMessages.AnalyticsRequestStarted, "OrganizationStats");

            var result = await _databaseReader.QuerySingleOrDefaultAsync<OrganizationStats>(
                MagicStrings.StoredProcedures.GetOrganizationStats);

            var stats = result;
            if (stats == null)
            {
                return Result.Failure<OrganizationStatsDto>(MagicStrings.ErrorMessages.AnalyticsDataNotFound);
            }

            var dto = _mapper.Map<OrganizationStatsDto>(stats);
            _logger.LogInformation(MagicStrings.LogMessages.AnalyticsRequestCompleted, "OrganizationStats", 1);
            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, MagicStrings.LogMessages.AnalyticsRequestFailed, "OrganizationStats", ex.Message);
            return Result.Failure<OrganizationStatsDto>(MagicStrings.ErrorMessages.InternalServerError);
        }
    }
}