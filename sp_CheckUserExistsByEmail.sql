CREATE PROCEDURE [dbo].[sp_CheckUserExistsByEmail]
    @Email NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT CASE WHEN COUNT(1) > 0 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS UserExists
    FROM Users 
    WHERE Email = @Email AND IsActive = 1;
END