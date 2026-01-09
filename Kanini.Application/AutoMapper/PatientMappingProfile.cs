using AutoMapper;
using Kanini.Application.DTOs.Patients;
using Kanini.Domain.Entities;

namespace Kanini.Application.AutoMapper;

public class PatientMappingProfile : Profile
{
    public PatientMappingProfile()
    {
        CreateMap<CreatePatientRequestDto, PatientIdentifier>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.GlobalPatientId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

        CreateMap<PatientIdentifier, PatientResponseDto>()
            .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.SourceOrganization != null ? src.SourceOrganization.Name : string.Empty))
            .ForMember(dest => dest.Message, opt => opt.Ignore());
    }
}