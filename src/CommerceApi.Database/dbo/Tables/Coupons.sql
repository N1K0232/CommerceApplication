CREATE TABLE [dbo].[Coupons]
(
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [Code] NVARCHAR(MAX) NOT NULL,
    [StartDate] DATETIME NOT NULL,
    [ExpirationDate] DATETIME NOT NULL,
    [Status] NVARCHAR(50) NOT NULL,
    [IsValid] BIT NOT NULL,
    [SecurityStamp] NVARCHAR(MAX) NOT NULL,
    [ConcurrencyStamp] NVARCHAR(MAX) NOT NULL,
    [CreationDate] DATE NOT NULL DEFAULT getutcdate(),
    [CreationTime] TIME(7) NOT NULL,
    [LastModificationDate] DATE NULL,
    [LastModificationTime] TIME(7) NULL,
    [IsDeleted] BIT NOT NULL,
    [DeletedDate] DATE NULL,
    [DeletedTime] TIME(7) NULL,

    PRIMARY KEY([Id]),
    FOREIGN KEY([UserId]) REFERENCES AspNetUsers([Id])
)