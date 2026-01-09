CREATE PROCEDURE sp_GetUserActivityStats
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