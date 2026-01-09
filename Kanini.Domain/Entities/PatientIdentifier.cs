using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Kanini.Common.Attributes;

namespace Kanini.Domain.Entities;

[Table("PatientIdentifiers")]
[Index(nameof(GlobalPatientId), nameof(SourceOrganizationId), IsUnique = true)]
[Index(nameof(LastNameHash), nameof(DateOfBirthHash))]
public class PatientIdentifier : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    public Guid GlobalPatientId { get; set; }

    [Required]
    [ForeignKey(nameof(SourceOrganization))]
    public Guid SourceOrganizationId { get; set; }

    [Required]
    [MaxLength(200)]
    [Encrypted]
    public string LocalPatientId { get; set; } = null!;

    [MaxLength(200)]
    [Encrypted]
    public string? LastName { get; set; }

    [MaxLength(200)]
    [Encrypted]
    public string? FirstName { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [MaxLength(64)]
    public string? LastNameHash { get; set; }

    [MaxLength(64)]
    public string? FirstNameHash { get; set; }

    [MaxLength(64)]
    public string? DateOfBirthHash { get; set; }

    // Navigation Properties
    public Organization SourceOrganization { get; set; } = null!;
}
