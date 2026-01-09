-- Get all users
CREATE PROCEDURE sp_GetAllUsers
AS
BEGIN
    SELECT UserId, Email, PasswordHash, Role, OrganizationId, IsActive, LastLogin,
           CreatedBy, CreatedOn, UpdatedBy, UpdatedOn
    FROM Users 
    WHERE IsActive = 1
    ORDER BY CreatedOn DESC -- Rule 9: Recent records first
END