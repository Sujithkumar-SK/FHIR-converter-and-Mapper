CREATE PROCEDURE sp_SearchPatientsByNameAndDob
    @FirstNameHash NVARCHAR(64),
    @LastNameHash NVARCHAR(64),
    @DateOfBirthHash NVARCHAR(64)
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
    WHERE LastNameHash = @LastNameHash 
      AND FirstNameHash = @FirstNameHash
      AND DateOfBirthHash = @DateOfBirthHash
    ORDER BY CreatedOn DESC
END