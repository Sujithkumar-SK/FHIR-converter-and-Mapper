using Kanini.Application.DTOs.Conversion;
using Kanini.Common.Results;

namespace Kanini.Application.Services.Conversion;

public interface IFhirConversionService
{
    System.Threading.Tasks.Task<Result<ConversionStatusResponseDto>> StartConversionAsync(StartConversionRequestDto request, Guid userId);
    System.Threading.Tasks.Task<Result<ConversionStatusResponseDto>> GetConversionStatusAsync(Guid jobId);
    System.Threading.Tasks.Task<Result<FhirBundlePreviewDto>> GetFhirPreviewAsync(Guid jobId);
    System.Threading.Tasks.Task<Result<byte[]>> DownloadFhirBundleAsync(Guid jobId);
    System.Threading.Tasks.Task<Result<IEnumerable<ConversionStatusResponseDto>>> GetConversionHistoryAsync(Guid userId);
    System.Threading.Tasks.Task<Result<ConversionStatusResponseDto>> GetConversionByRequestIdAsync(Guid requestId);
    System.Threading.Tasks.Task<Result> ResetConversionJobAsync(Guid jobId, Guid userId);
}