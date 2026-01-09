CREATE PROCEDURE sp_GetPendingDataRequests
    @SourceOrganizationId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        dr.RequestId,
        dr.GlobalPatientId,
        dr.RequestingUserId,
        dr.RequestingOrganizationId,
        dr.SourceOrganizationId,
        dr.Status,
        dr.Notes,
        dr.ApprovedAt,
        dr.ApprovedByUserId,
        dr.ExpiresAt,
        dr.CreatedBy,
        dr.CreatedOn,
        dr.UpdatedBy,
        dr.UpdatedOn,
        -- Requesting Organization
        ro.Name as RequestingOrganizationName,
        -- Source Organization  
        so.Name as SourceOrganizationName,
        -- Requesting User
        ru.Email as RequestingUserEmail,
        -- Approved By User
        au.Email as ApprovedByUserEmail
    FROM DataRequests dr
    INNER JOIN Organizations ro ON dr.RequestingOrganizationId = ro.OrganizationId
    INNER JOIN Organizations so ON dr.SourceOrganizationId = so.OrganizationId
    INNER JOIN Users ru ON dr.RequestingUserId = ru.UserId
    LEFT JOIN Users au ON dr.ApprovedByUserId = au.UserId
    WHERE 
        dr.SourceOrganizationId = @SourceOrganizationId
        AND dr.Status = 1 -- Pending status
        AND dr.ExpiresAt > GETUTCDATE()
    ORDER BY dr.CreatedOn DESC;
END