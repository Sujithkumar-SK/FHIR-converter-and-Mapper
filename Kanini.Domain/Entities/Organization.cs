using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Kanini.Domain.Enums;
using Kanini.Common.Attributes;

namespace Kanini.Domain.Entities;

[Table("Organizations")]
[Index(nameof(OrganizationId), IsUnique = true)]
public class Organization : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid OrganizationId { get; set; }

    [Required]
    [MaxLength(500)]
    [Encrypted]
    public string Name { get; set; } = null!;

    [Required]
    public OrganizationType Type { get; set; }

    [Required]
    [MaxLength(300)]
    [Encrypted]
    [EmailAddress]
    public string ContactEmail { get; set; } = null!;

    [MaxLength(200)]
    [Encrypted]
    public string? ContactPhone { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<PatientIdentifier> PatientIdentifiers { get; set; } = new List<PatientIdentifier>();
    public ICollection<DataRequest> SourceDataRequests { get; set; } = new List<DataRequest>();
    public ICollection<DataRequest> RequestingDataRequests { get; set; } = new List<DataRequest>();
}