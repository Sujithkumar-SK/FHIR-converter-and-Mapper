CREATE PROCEDURE sp_GetPatientByGlobalId
    @GlobalPatientId UNIQUEIDENTIFIER
AS
BEGIN
    SELECT TOP 1
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
    WHERE GlobalPatientId = @GlobalPatientId
    ORDER BY CreatedOn DESC
END