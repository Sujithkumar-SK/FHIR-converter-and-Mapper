namespace Kanini.Application.DTOs.Patients;

public class PatientResponseDto
{
    public Guid Id { get; set; }
    public Guid GlobalPatientId { get; set; }
    public Guid SourceOrganizationId { get; set; }
    public string LocalPatientId { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime CreatedOn { get; set; }
    public string? OrganizationName { get; set; }
    public string Message { get; set; } = string.Empty;
}