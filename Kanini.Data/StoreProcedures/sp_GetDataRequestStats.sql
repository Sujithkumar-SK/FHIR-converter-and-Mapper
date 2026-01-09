CREATE PROCEDURE sp_GetDataRequestStats
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