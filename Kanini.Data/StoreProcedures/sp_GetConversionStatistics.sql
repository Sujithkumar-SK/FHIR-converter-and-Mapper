CREATE PROCEDURE sp_GetConversionStatistics
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