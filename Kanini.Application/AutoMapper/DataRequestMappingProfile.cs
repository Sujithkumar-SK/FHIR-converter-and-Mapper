using AutoMapper;
using Kanini.Application.DTOs.DataRequests;
using Kanini.Domain.Entities;

namespace Kanini.Application.AutoMapper;

public class DataRequestMappingProfile : Profile
{
    public DataRequestMappingProfile()
    {
        CreateMap<CreateDataRequestDto, DataRequest>()
            .ForMember(dest => dest.RequestId, opt => opt.Ignore())
            .ForMember(dest => dest.RequestingUserId, opt => opt.Ignore())
            .ForMember(dest => dest.RequestingOrganizationId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Domain.Enums.DataRequestStatus.Pending))
            .ForMember(dest => dest.ExpiresAt, opt => opt.MapFrom(src => DateTime.UtcNow.AddDays(7)));

        CreateMap<DataRequest, DataRequestResponseDto>()
            .ForMember(dest => dest.RequestingOrganizationName, opt => opt.MapFrom(src => src.RequestingOrganization.Name))
            .ForMember(dest => dest.SourceOrganizationName, opt => opt.MapFrom(src => src.SourceOrganization.Name))
            .ForMember(dest => dest.RequestingUserEmail, opt => opt.MapFrom(src => src.RequestingUser.Email))
            .ForMember(dest => dest.ApprovedByUserEmail, opt => opt.MapFrom(src => src.ApprovedByUser != null ? src.ApprovedByUser.Email : null));
    }
}