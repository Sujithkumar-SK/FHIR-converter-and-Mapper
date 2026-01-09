CREATE PROCEDURE sp_GetDataRequestsByOrganization
    @OrganizationId UNIQUEIDENTIFIER,
    @IsRequesting BIT = 1 -- 1 for requesting org, 0 for source org
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
        (@IsRequesting = 1 AND dr.RequestingOrganizationId = @OrganizationId)
        OR 
        (@IsRequesting = 0 AND dr.SourceOrganizationId = @OrganizationId)
    ORDER BY dr.CreatedOn DESC;
END