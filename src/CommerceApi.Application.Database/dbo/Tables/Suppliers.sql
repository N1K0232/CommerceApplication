CREATE TABLE [dbo].[Suppliers]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [CompanyName] NVARCHAR(100) NOT NULL,
    [ContactName] NVARCHAR(100) NOT NULL,
    [City] NVARCHAR(50) NOT NULL,
    [CreationDate] DATETIME NOT NULL DEFAULT getutcdate(),
    [LastModificationDate] DATETIME NULL,

    PRIMARY KEY([Id])
)