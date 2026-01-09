CREATE PROCEDURE [dbo].[sp_GetUserByEmail]
    @Email NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT UserId, Email, PasswordHash, Role, OrganizationId, IsActive, LastLogin,
           CreatedBy, CreatedOn, UpdatedBy, UpdatedOn
    FROM Users 
    WHERE Email = @Email AND IsActive = 1
    ORDER BY CreatedOn DESC;
END