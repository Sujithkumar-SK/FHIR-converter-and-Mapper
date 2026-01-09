using System.ComponentModel.DataAnnotations;
using Kanini.Domain.Enums;

namespace Kanini.Application.DTOs.DataRequests;

public class ApproveDataRequestDto
{
    [Required]
    public DataRequestStatus Status { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}