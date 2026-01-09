CREATE PROCEDURE sp_SearchPatientsByTerm
    @SearchTerm NVARCHAR(400),
    @DateOfBirth DATE = NULL
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
    WHERE (
        LocalPatientId LIKE '%' + @SearchTerm + '%' OR
        FirstName LIKE '%' + @SearchTerm + '%' OR
        LastName LIKE '%' + @SearchTerm + '%' OR
        (FirstName + ' ' + LastName) LIKE '%' + @SearchTerm + '%'
    )
    AND (@DateOfBirth IS NULL OR DateOfBirth = @DateOfBirth)
    ORDER BY CreatedOn DESC
END