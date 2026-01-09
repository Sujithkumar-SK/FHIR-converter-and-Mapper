using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kanini.Domain.Enums;
using Kanini.Common.Attributes;

namespace Kanini.Domain.Entities;

[Table("ConversionJobs")]
public class ConversionJob : BaseEntity
{
    [Key]
    public Guid JobId { get; set; } = Guid.NewGuid();

    [Required]
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(DataRequest))]
    public Guid? RequestId { get; set; }

    [Required]
    public InputFormat InputFormat { get; set; }

    [Required]
    public ConversionStatus Status { get; set; } = ConversionStatus.Processing;

    [MaxLength(2000)]
    [Encrypted]
    public string? ErrorMessage { get; set; }

    public int PatientsCount { get; set; } = 0;

    public int ObservationsCount { get; set; } = 0;

    public DateTime? CompletedAt { get; set; }

    [MaxLength(200)]
    [Encrypted]
    public string? OriginalFileName { get; set; }

    public long? FileSizeBytes { get; set; }

    public long? ProcessingTimeMs { get; set; }
    public User User { get; set; } = null!;
    public DataRequest? DataRequest { get; set; }
}