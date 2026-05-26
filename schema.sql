CREATE TABLE Roles (
    Id          INT           NOT NULL IDENTITY(1,1),
    Name        NVARCHAR(64)  NOT NULL,
    Description NVARCHAR(256) NULL,
    CONSTRAINT PK_Roles      PRIMARY KEY (Id),
    CONSTRAINT UQ_Roles_Name UNIQUE      (Name)
);

CREATE TABLE Users (
    Id           INT           NOT NULL IDENTITY(1,1),
    UserName     NVARCHAR(64)  NOT NULL,
    Email        NVARCHAR(256) NOT NULL,
    PasswordHash NVARCHAR(256) NOT NULL,
    IsActive     BIT           NOT NULL DEFAULT 1,
    CreatedAt    DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Users          PRIMARY KEY (Id),
    CONSTRAINT UQ_Users_UserName UNIQUE      (UserName),
    CONSTRAINT UQ_Users_Email    UNIQUE      (Email)
);

CREATE TABLE UserRoles (
    UserId     INT       NOT NULL,
    RoleId     INT       NOT NULL,
    AssignedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_UserRoles              PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_UserRoles_Users_UserId FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserRoles_Roles_RoleId FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE NO ACTION
);

-- Seed roles
SET IDENTITY_INSERT Roles ON;
INSERT INTO Roles (Id, Name, Description) VALUES
    (1, 'SuperAdmin', 'Full system access; can create users.'),
    (2, 'Admin',      'Manages users and roles.'),
    (3, 'User',       'Standard authenticated user.');
SET IDENTITY_INSERT Roles OFF;
