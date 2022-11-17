CREATE TABLE [dbo].[Orders]
(
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [OrderDate] DATE NOT NULL,
    [OrderTime] TIME NOT NULL,
    [Status] NVARCHAR(10) NOT NULL,
    [CreationDate] DATETIME NOT NULL,
    [UpdatedDate] DATETIME NULL,
    [IsDeleted] BIT NOT NULL,
    [DeletedDate] DATETIME NULL,

    PRIMARY KEY(Id),
    FOREIGN KEY(UserId) REFERENCES AspNetUsers(Id)
)