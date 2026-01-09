-- Get user by ID
CREATE PROCEDURE sp_GetUserById
    @UserId INT
AS
BEGIN
    SELECT UserId, Email, PasswordHash, Role, OrganizationId, IsActive, LastLogin,
           CreatedBy, CreatedOn, UpdatedBy, UpdatedOn
    FROM Users 
    WHERE UserId = @UserId AND IsActive = 1
END