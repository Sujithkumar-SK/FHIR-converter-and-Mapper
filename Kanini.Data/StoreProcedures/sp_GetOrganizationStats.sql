CREATE PROCEDURE sp_GetOrganizationStats
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @TotalOrganizations INT = (SELECT COUNT(*) FROM Organizations WHERE IsActive = 1);
    DECLARE @ActiveOrganizations INT = (
        SELECT COUNT(DISTINCT o.OrganizationId) 
        FROM Organizations o
        INNER JOIN Users u ON o.OrganizationId = u.OrganizationId
        WHERE o.IsActive = 1 AND u.LastLogin >= DATEADD(DAY, -30, GETUTCDATE())
    );
    DECLARE @HospitalCount INT = (SELECT COUNT(*) FROM Organizations WHERE IsActive = 1 AND Type = 1);
    DECLARE @ClinicCount INT = (SELECT COUNT(*) FROM Organizations WHERE IsActive = 1 AND Type = 2);
    
    SELECT 
        @TotalOrganizations AS TotalOrganizations,
        @ActiveOrganizations AS ActiveOrganizations,
        @HospitalCount AS HospitalCount,
        @ClinicCount AS ClinicCount;
END