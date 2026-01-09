CREATE PROCEDURE [dbo].[sp_GetUserById]
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT UserId, Email, PasswordHash, Role, OrganizationId, IsActive, LastLogin,
           CreatedBy, CreatedOn, UpdatedBy, UpdatedOn
    FROM Users 
    WHERE UserId = @UserId AND IsActive = 1;
END