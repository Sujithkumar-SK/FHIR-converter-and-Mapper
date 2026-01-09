using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Kanini.Domain.Enums;
using Kanini.Common.Attributes;

namespace Kanini.Domain.Entities;

[Table("Users")]
[Index(nameof(Email), IsUnique = true)]
public class User : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(150)]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [MaxLength(256)]
    public string PasswordHash { get; set; } = null!;

    [Required]
    public UserRole Role { get; set; }

    [ForeignKey(nameof(Organization))]
    public Guid? OrganizationId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? LastLogin { get; set; }

    // Navigation Properties
    public Organization? Organization { get; set; }
    
    public ICollection<ConversionJob> ConversionJobs { get; set; } = new List<ConversionJob>();
    public ICollection<DataRequest> RequestedDataRequests { get; set; } = new List<DataRequest>();
    public ICollection<DataRequest> ApprovedDataRequests { get; set; } = new List<DataRequest>();
}
