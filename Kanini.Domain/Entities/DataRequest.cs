using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kanini.Domain.Enums;
using Kanini.Common.Attributes;

namespace Kanini.Domain.Entities;

[Table("DataRequests")]
public class DataRequest : BaseEntity
{
    [Key]
    public Guid RequestId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid GlobalPatientId { get; set; }

    [Required]
    [ForeignKey(nameof(RequestingUser))]
    public Guid RequestingUserId { get; set; }

    [Required]
    [ForeignKey(nameof(RequestingOrganization))]
    public Guid RequestingOrganizationId { get; set; }

    [Required]
    [ForeignKey(nameof(SourceOrganization))]
    public Guid SourceOrganizationId { get; set; }

    [Required]
    public DataRequestStatus Status { get; set; } = DataRequestStatus.Pending;

    [MaxLength(1000)]
    [Encrypted]
    public string? Notes { get; set; }

    public DateTime? ApprovedAt { get; set; }

    [ForeignKey(nameof(ApprovedByUser))]
    public Guid? ApprovedByUserId { get; set; }

    [Required]
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);

    // Navigation Properties
    public User RequestingUser { get; set; } = null!;
    public User? ApprovedByUser { get; set; }
    public Organization RequestingOrganization { get; set; } = null!;
    public Organization SourceOrganization { get; set; } = null!;
    public ICollection<ConversionJob> ConversionJobs { get; set; } = new List<ConversionJob>();
}