CREATE TABLE [dbo].[Suppliers]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [CompanyName] NVARCHAR(256) NOT NULL,
    [ContactName] NVARCHAR(256) NOT NULL,
    [City] NVARCHAR(100) NOT NULL,
    [CreationDate] DATETIME NOT NULL,
    [UpdatedDate] DATETIME NULL,

    PRIMARY KEY(Id)
)