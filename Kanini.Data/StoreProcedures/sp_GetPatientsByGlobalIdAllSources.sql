CREATE PROCEDURE sp_GetPatientsByGlobalIdAllSources
    @GlobalPatientId UNIQUEIDENTIFIER
AS
BEGIN
    SELECT 
        pi.Id,
        pi.GlobalPatientId,
        pi.SourceOrganizationId,
        pi.LocalPatientId,
        pi.LastName,
        pi.FirstName,
        pi.DateOfBirth,
        pi.LastNameHash,
        pi.FirstNameHash,
        pi.DateOfBirthHash,
        pi.CreatedBy,
        pi.CreatedOn,
        pi.UpdatedBy,
        pi.UpdatedOn,
        o.Name as OrganizationName
    FROM PatientIdentifiers pi
    LEFT JOIN Organizations o ON pi.SourceOrganizationId = o.OrganizationId
    WHERE pi.GlobalPatientId = @GlobalPatientId
    ORDER BY pi.CreatedOn DESC
END