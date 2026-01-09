using AutoMapper;
using Kanini.Application.DTOs.Files;
using Kanini.Domain.Entities;

namespace Kanini.Application.AutoMapper;

public class FileMappingProfile : Profile
{
    public FileMappingProfile()
    {
        CreateMap<ConversionJob, FileUploadResponseDto>()
            .ForMember(dest => dest.FileId, opt => opt.MapFrom(src => src.JobId))
            .ForMember(dest => dest.DetectedFormat, opt => opt.MapFrom(src => src.InputFormat))
            .ForMember(dest => dest.UploadedAt, opt => opt.MapFrom(src => src.CreatedOn))
            .ForMember(dest => dest.ExpiresAt, opt => opt.MapFrom(src => src.CreatedOn.AddHours(1)));
    }
}