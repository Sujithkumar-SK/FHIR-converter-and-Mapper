using AutoMapper;
using Kanini.Application.DTOs.Conversion;
using Kanini.Domain.Entities;

namespace Kanini.Application.AutoMapper;

public class ConversionMappingProfile : Profile
{
    public ConversionMappingProfile()
    {
        CreateMap<ConversionJob, ConversionStatusResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Progress, opt => opt.MapFrom(src => 
                src.Status == Domain.Enums.ConversionStatus.Completed ? 100 : 
                src.Status == Domain.Enums.ConversionStatus.Processing ? 50 : 0))
            .ForMember(dest => dest.PatientsProcessed, opt => opt.MapFrom(src => src.PatientsCount))
            .ForMember(dest => dest.ObservationsProcessed, opt => opt.MapFrom(src => src.ObservationsCount));
    }
}