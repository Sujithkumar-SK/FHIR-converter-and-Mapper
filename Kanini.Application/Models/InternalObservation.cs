namespace Kanini.Application.Models;

public class InternalObservation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string PatientId { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Display { get; set; }
    public string? System { get; set; } = "http://loinc.org";
    public decimal? ValueQuantity { get; set; }
    public string? ValueUnit { get; set; }
    public string? ValueString { get; set; }
    public DateTime? EffectiveDateTime { get; set; }
    public string Status { get; set; } = "final";
}