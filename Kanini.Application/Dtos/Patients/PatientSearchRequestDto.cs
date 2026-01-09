using System.ComponentModel.DataAnnotations;

namespace Kanini.Application.DTOs.Patients;

public class PatientSearchRequestDto
{
    [Required(ErrorMessage = "Search term is required")]
    [MaxLength(400, ErrorMessage = "Search term cannot exceed 400 characters")]
    public string SearchTerm { get; set; } = null!;

    public DateTime? DateOfBirth { get; set; }
}