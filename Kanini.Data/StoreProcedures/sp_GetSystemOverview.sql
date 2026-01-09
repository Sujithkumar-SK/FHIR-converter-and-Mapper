CREATE PROCEDURE sp_GetSystemOverview
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @TotalUsers INT = (SELECT COUNT(*) FROM Users WHERE IsActive = 1);
    DECLARE @ActiveUsers INT = (SELECT COUNT(*) FROM Users WHERE IsActive = 1 AND LastLogin >= DATEADD(DAY, -30, GETUTCDATE()));
    DECLARE @TotalOrganizations INT = (SELECT COUNT(*) FROM Organizations WHERE IsActive = 1);
    DECLARE @TotalConversions INT = (SELECT COUNT(*) FROM ConversionJobs);
    DECLARE @TotalDataRequests INT = (SELECT COUNT(*) FROM DataRequests);
    DECLARE @PendingDataRequests INT = (SELECT COUNT(*) FROM DataRequests WHERE Status = 1);
    DECLARE @TotalPatients INT = (SELECT COUNT(DISTINCT GlobalPatientId) FROM PatientIdentifiers);
    
    SELECT 
        @TotalUsers AS TotalUsers,
        @ActiveUsers AS ActiveUsers,
        @TotalOrganizations AS TotalOrganizations,
        @TotalConversions AS TotalConversions,
        @TotalDataRequests AS TotalDataRequests,
        @PendingDataRequests AS PendingDataRequests,
        @TotalPatients AS TotalPatients,
        GETUTCDATE() AS LastUpdated;
END