SELECT 
    u.UserName,
    r.Name AS RoleName,
    ur.AssignedAt
FROM USER_ROLES ur
INNER JOIN USERS u ON ur.UserId = u.Id
INNER JOIN ROLES r ON ur.RoleId = r.Id
ORDER BY u.UserName, r.Name;
