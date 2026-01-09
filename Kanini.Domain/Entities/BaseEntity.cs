using System.ComponentModel.DataAnnotations;
using Kanini.Common.Attributes;

namespace Kanini.Domain.Entities;

public abstract class BaseEntity
{
    [MaxLength(100)]
    [Encrypted]
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    [Encrypted]
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedOn { get; set; }
}