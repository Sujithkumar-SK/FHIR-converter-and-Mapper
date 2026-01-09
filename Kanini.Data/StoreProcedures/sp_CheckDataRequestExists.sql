CREATE PROCEDURE sp_CheckDataRequestExists
    @GlobalPatientId UNIQUEIDENTIFIER,
    @RequestingOrganizationId UNIQUEIDENTIFIER,
    @SourceOrganizationId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT CAST(CASE 
        WHEN EXISTS (
            SELECT 1 
            FROM DataRequests 
            WHERE GlobalPatientId = @GlobalPatientId
                AND RequestingOrganizationId = @RequestingOrganizationId
                AND SourceOrganizationId = @SourceOrganizationId
                AND Status IN (1, 2) -- Pending or Approved
                AND ExpiresAt > GETUTCDATE()
        ) 
        THEN 1 
        ELSE 0 
    END AS BIT) as RequestExists;
END