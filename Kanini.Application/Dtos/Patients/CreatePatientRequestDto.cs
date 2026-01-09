using System.ComponentModel.DataAnnotations;

namespace Kanini.Application.DTOs.Patients;

public class CreatePatientRequestDto
{
    [Required(ErrorMessage = "Local Patient ID is required")]
    [MaxLength(200, ErrorMessage = "Local Patient ID cannot exceed 200 characters")]
    public string LocalPatientId { get; set; } = null!;

    [MaxLength(200, ErrorMessage = "First name cannot exceed 200 characters")]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required")]
    [MaxLength(200, ErrorMessage = "Last name cannot exceed 200 characters")]
    public string LastName { get; set; } = null!;

    public DateTime? DateOfBirth { get; set; }

    [Required(ErrorMessage = "Organization ID is required")]
    public Guid SourceOrganizationId { get; set; }
}