-- Check if user exists by email
CREATE PROCEDURE sp_CheckUserExistsByEmail
    @Email NVARCHAR(150)
AS
BEGIN
    SELECT CASE WHEN EXISTS(
        SELECT 1 FROM Users WHERE Email = @Email AND IsActive = 1
    ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS UserExists
END