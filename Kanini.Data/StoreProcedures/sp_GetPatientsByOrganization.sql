CREATE PROCEDURE sp_GetPatientsByOrganization
    @OrganizationId UNIQUEIDENTIFIER
AS
BEGIN
    SELECT 
        Id,
        GlobalPatientId,
        SourceOrganizationId,
        LocalPatientId,
        LastName,
        FirstName,
        DateOfBirth,
        LastNameHash,
        FirstNameHash,
        DateOfBirthHash,
        CreatedBy,
        CreatedOn,
        UpdatedBy,
        UpdatedOn
    FROM PatientIdentifiers
    WHERE SourceOrganizationId = @OrganizationId
    ORDER BY CreatedOn DESC
END