using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using NUnit.Framework;

namespace Kanini.Tests;

public class FhirIntegrationTest
{
    [Test]
    public void CanCreateFhirBundle()
    {
        // Arrange
        var patient = new Patient
        {
            Id = "patient-1",
            Identifier = { new Identifier("http://hospital.org", "12345") },
            Name = { new HumanName().WithGiven("John").AndFamily("Doe") }
        };

        var bundle = new Bundle
        {
            Id = "test-bundle",
            Type = Bundle.BundleType.Collection,
            Entry = { new Bundle.EntryComponent { Resource = patient } }
        };

        // Act
        var serializer = new FhirJsonSerializer();
        var json = serializer.SerializeToString(bundle);

        // Assert
        Assert.That(json, Is.Not.Null);
        Assert.That(json, Does.Contain("Patient"));
        Assert.That(json, Does.Contain("John"));
        Assert.That(json, Does.Contain("Doe"));
    }
}