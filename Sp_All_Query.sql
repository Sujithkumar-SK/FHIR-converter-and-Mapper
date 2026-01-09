select * from Users
select * from Organizations
select * from PatientIdentifiers
select * from DataRequests
select * from ConversionJobs
--delete from Users
--delete from Organizations where Name='Admin'
--delete from DataRequests where RequestId='5895DCAD-31C5-401C-A803-4ED5139EBB33'
--delete from ConversionJobs where InputFormat=1
GO
-------------------------------------------------------------
CREATE OR ALTER PROCEDURE [dbo].[sp_CheckUserExistsByEmail]
    @Email NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT CASE WHEN COUNT(1) > 0 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS UserExists
    FROM Users 
    WHERE Email = @Email AND IsActive = 1;
END
GO
---------------------------------------------------------------------
CREATE OR ALTER PROCEDURE [dbo].[sp_GetAllUsers]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT UserId, Email, PasswordHash, Role, OrganizationId, IsActive, LastLogin,
           CreatedBy, CreatedOn, UpdatedBy, UpdatedOn
    FROM Users 
    WHERE IsActive = 1
    ORDER BY CreatedOn DESC;
END
GO
----------------------------------------------------------------------
CREATE OR ALTER PROCEDURE [dbo].[sp_GetUserById]
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT UserId, Email, PasswordHash, Role, OrganizationId, IsActive, LastLogin,
           CreatedBy, CreatedOn, UpdatedBy, UpdatedOn
    FROM Users 
    WHERE UserId = @UserId AND IsActive = 1;
END
GO
----------------------------------------------------------------------
-- Get user by email with organization name
CREATE OR ALTER PROCEDURE sp_GetUserByEmail
    @Email NVARCHAR(150)
AS
BEGIN
    SELECT u.UserId, u.Email, u.PasswordHash, u.Role, u.OrganizationId, u.IsActive, u.LastLogin, 
           u.CreatedBy, u.CreatedOn, u.UpdatedBy, u.UpdatedOn,
           o.Name as OrganizationName
    FROM Users u
    LEFT JOIN Organizations o ON u.OrganizationId = o.OrganizationId
    WHERE u.Email = @Email AND u.IsActive = 1
    ORDER BY u.CreatedOn DESC
END
GO
----------------------------------------------------------------------
CREATE OR ALTER PROCEDURE sp_SearchPatientsByNameAndDob
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
GO
------------------------------------------------------
CREATE OR ALTER PROCEDURE sp_GetPatientByGlobalId
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
GO
------------------------------------------------------
CREATE OR ALTER PROCEDURE sp_GetPatientsByGlobalIdAllSources
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
GO
------------------------------------------------------
CREATE OR ALTER PROCEDURE sp_GetPatientsByOrganization
    @OrganizationId UNIQUEIDENTIFIER
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
    WHERE SourceOrganizationId = @OrganizationId
    ORDER BY CreatedOn DESC
END
GO
------------------------------------------------------
CREATE OR ALTER PROCEDURE sp_SearchPatientsByTerm
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
GO
------------------------------------------------------
CREATE OR ALTER PROCEDURE sp_GetAllPatients
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
GO
------------------------------------------------------
CREATE OR ALTER PROCEDURE sp_GetDataRequestsByOrganization
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
GO
------------------------------------------------------
CREATE OR ALTER PROCEDURE sp_GetDataRequestById
    @RequestId UNIQUEIDENTIFIER
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
    WHERE dr.RequestId = @RequestId;
END
GO
------------------------------------------------------
CREATE OR ALTER PROCEDURE sp_GetPendingDataRequests
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
GO
------------------------------------------------------
CREATE OR ALTER PROCEDURE sp_CheckDataRequestExists
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
GO
------------------------------------------------------
CREATE OR ALTER PROCEDURE sp_GetSystemOverview
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
GO
------------------------------------------------------
CREATE OR ALTER PROCEDURE sp_GetConversionStatistics
    @StartDate DATETIME2,
    @EndDate DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Main statistics
    DECLARE @TotalConversions INT = (
        SELECT COUNT(*) FROM ConversionJobs 
        WHERE CreatedOn >= @StartDate AND CreatedOn <= @EndDate
    );
    
    DECLARE @SuccessfulConversions INT = (
        SELECT COUNT(*) FROM ConversionJobs 
        WHERE CreatedOn >= @StartDate AND CreatedOn <= @EndDate AND Status = 2
    );
    
    DECLARE @FailedConversions INT = (
        SELECT COUNT(*) FROM ConversionJobs 
        WHERE CreatedOn >= @StartDate AND CreatedOn <= @EndDate AND Status = 3
    );
    
    DECLARE @ProcessingConversions INT = (
        SELECT COUNT(*) FROM ConversionJobs 
        WHERE CreatedOn >= @StartDate AND CreatedOn <= @EndDate AND Status = 1
    );
    
    DECLARE @SuccessRate DECIMAL(5,2) = 
        CASE WHEN @TotalConversions > 0 
        THEN CAST(@SuccessfulConversions AS DECIMAL(5,2)) / @TotalConversions * 100 
        ELSE 0 END;
    
    DECLARE @AverageProcessingTimeMs BIGINT = (
        SELECT ISNULL(AVG(ProcessingTimeMs), 0) FROM ConversionJobs 
        WHERE CreatedOn >= @StartDate AND CreatedOn <= @EndDate AND ProcessingTimeMs IS NOT NULL
    );
    
    SELECT 
        @TotalConversions AS TotalConversions,
        @SuccessfulConversions AS SuccessfulConversions,
        @FailedConversions AS FailedConversions,
        @ProcessingConversions AS ProcessingConversions,
        @SuccessRate AS SuccessRate,
        @AverageProcessingTimeMs AS AverageProcessingTimeMs;
END
GO
------------------------------------------------------
CREATE OR ALTER PROCEDURE sp_GetUserActivityStats
    @StartDate DATETIME2,
    @EndDate DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @TotalUsers INT = (SELECT COUNT(*) FROM Users WHERE IsActive = 1);
    DECLARE @ActiveUsers INT = (
        SELECT COUNT(*) FROM Users 
        WHERE IsActive = 1 AND LastLogin >= @StartDate
    );
    DECLARE @InactiveUsers INT = @TotalUsers - @ActiveUsers;
    
    SELECT 
        @TotalUsers AS TotalUsers,
        @ActiveUsers AS ActiveUsers,
        @InactiveUsers AS InactiveUsers,
        GETUTCDATE() AS LastUpdated;
END
GO
------------------------------------------------------
CREATE OR ALTER PROCEDURE sp_GetDataRequestStats
    @StartDate DATETIME2,
    @EndDate DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @TotalRequests INT = (
        SELECT COUNT(*) FROM DataRequests 
        WHERE CreatedOn >= @StartDate AND CreatedOn <= @EndDate
    );
    
    DECLARE @PendingRequests INT = (
        SELECT COUNT(*) FROM DataRequests 
        WHERE CreatedOn >= @StartDate AND CreatedOn <= @EndDate AND Status = 1
    );
    
    DECLARE @ApprovedRequests INT = (
        SELECT COUNT(*) FROM DataRequests 
        WHERE CreatedOn >= @StartDate AND CreatedOn <= @EndDate AND Status = 2
    );
    
    DECLARE @RejectedRequests INT = (
        SELECT COUNT(*) FROM DataRequests 
        WHERE CreatedOn >= @StartDate AND CreatedOn <= @EndDate AND Status = 3
    );
    
    DECLARE @CompletedRequests INT = (
        SELECT COUNT(*) FROM DataRequests 
        WHERE CreatedOn >= @StartDate AND CreatedOn <= @EndDate AND Status = 5
    );
    
    DECLARE @ExpiredRequests INT = (
        SELECT COUNT(*) FROM DataRequests 
        WHERE CreatedOn >= @StartDate AND CreatedOn <= @EndDate AND Status = 6
    );
    
    DECLARE @ApprovalRate DECIMAL(5,2) = 
        CASE WHEN @TotalRequests > 0 
        THEN CAST(@ApprovedRequests AS DECIMAL(5,2)) / @TotalRequests * 100 
        ELSE 0 END;
    
    DECLARE @AverageProcessingHours FLOAT = (
        SELECT ISNULL(AVG(DATEDIFF(HOUR, CreatedOn, ApprovedAt)), 0) 
        FROM DataRequests 
        WHERE CreatedOn >= @StartDate AND CreatedOn <= @EndDate AND ApprovedAt IS NOT NULL
    );
    
    SELECT 
        @TotalRequests AS TotalRequests,
        @PendingRequests AS PendingRequests,
        @ApprovedRequests AS ApprovedRequests,
        @RejectedRequests AS RejectedRequests,
        @CompletedRequests AS CompletedRequests,
        @ExpiredRequests AS ExpiredRequests,
        @ApprovalRate AS ApprovalRate,
        @AverageProcessingHours AS AverageProcessingHours;
END
GO
------------------------------------------------------
CREATE OR ALTER PROCEDURE sp_GetOrganizationStats
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
GO
------------------------------------------------------