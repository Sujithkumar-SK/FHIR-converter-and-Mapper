namespace Kanini.Application.Models;

public class InternalPatient
{
    public string Id { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public InternalAddress? Address { get; set; }
    public List<InternalIdentifier> Identifiers { get; set; } = new();
}

public class InternalAddress
{
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
}

public class InternalIdentifier
{
    public string System { get; set; } = null!;
    public string Value { get; set; } = null!;
}