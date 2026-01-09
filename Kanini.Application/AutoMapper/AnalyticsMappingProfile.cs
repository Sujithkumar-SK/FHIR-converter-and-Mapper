using AutoMapper;
using Kanini.Application.DTOs.Analytics;
using Kanini.Domain.Analytics;
using Kanini.Domain.Enums;

namespace Kanini.Application.AutoMapper;

public class AnalyticsMappingProfile : Profile
{
    public AnalyticsMappingProfile()
    {
        // Domain to DTO mappings
        CreateMap<SystemOverview, SystemOverviewDto>();
        CreateMap<ConversionStatistics, ConversionStatisticsDto>();
        CreateMap<UserActivityStats, UserActivityStatsDto>();
        CreateMap<DataRequestStats, DataRequestStatsDto>();
        CreateMap<OrganizationStats, OrganizationStatsDto>();
        
        // Enum mappings
        CreateMap<UserRole, UsersByRoleDto>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.ToString()));

        CreateMap<OrganizationType, OrganizationByTypeDto>()
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.ToString()));

        CreateMap<InputFormat, ConversionByFormatDto>()
            .ForMember(dest => dest.FormatName, opt => opt.MapFrom(src => src.ToString()));

        CreateMap<DataRequestStatus, RequestsByStatusDto>()
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.ToString()));
    }
}