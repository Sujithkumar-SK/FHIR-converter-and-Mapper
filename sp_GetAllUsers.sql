CREATE PROCEDURE [dbo].[sp_GetAllUsers]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT UserId, Email, PasswordHash, Role, OrganizationId, IsActive, LastLogin,
           CreatedBy, CreatedOn, UpdatedBy, UpdatedOn
    FROM Users 
    WHERE IsActive = 1
    ORDER BY CreatedOn DESC;
END