using System.ComponentModel.DataAnnotations;

namespace Kanini.Application.DTOs.DataRequests;

public class CreateDataRequestDto
{
    [Required]
    public Guid GlobalPatientId { get; set; }

    [Required]
    public Guid SourceOrganizationId { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}