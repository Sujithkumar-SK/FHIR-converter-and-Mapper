CREATE PROCEDURE sp_GetAllPatients
AS
BEGIN
    SELECT 
        p.Id,
        p.GlobalPatientId,
        p.SourceOrganizationId,
        p.LocalPatientId,
        p.LastName,
        p.FirstName,
        p.DateOfBirth,
        p.LastNameHash,
        p.FirstNameHash,
        p.DateOfBirthHash,
        p.CreatedBy,
        p.CreatedOn,
        p.UpdatedBy,
        p.UpdatedOn,
        o.Name as OrganizationName
    FROM PatientIdentifiers p
    LEFT JOIN Organizations o ON p.SourceOrganizationId = o.OrganizationId
    ORDER BY p.CreatedOn DESC
END