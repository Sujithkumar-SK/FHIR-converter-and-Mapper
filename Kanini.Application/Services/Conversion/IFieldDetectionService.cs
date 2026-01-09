using Kanini.Application.DTOs.Conversion;
using Kanini.Common.Results;

namespace Kanini.Application.Services.Conversion;

public interface IFieldDetectionService
{
    Task<Result<FieldDetectionResponseDto>> DetectFieldsAsync(Guid fileId);
    Task<Result<List<string>>> GetAvailableFhirFieldsAsync();
}