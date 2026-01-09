-- Get user by email with organization name
CREATE PROCEDURE sp_GetUserByEmail
    @Email NVARCHAR(150)
AS
BEGIN
    SELECT u.UserId, u.Email, u.PasswordHash, u.Role, u.OrganizationId, u.IsActive, u.LastLogin, 
           u.CreatedBy, u.CreatedOn, u.UpdatedBy, u.UpdatedOn,
           o.Name as OrganizationName
    FROM Users u
    LEFT JOIN Organizations o ON u.OrganizationId = o.OrganizationId
    WHERE u.Email = @Email AND u.IsActive = 1
    ORDER BY u.CreatedOn DESC
END