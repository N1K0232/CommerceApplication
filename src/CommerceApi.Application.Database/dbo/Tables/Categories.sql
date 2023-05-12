CREATE TABLE [dbo].[Categories]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [Name] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(512) NULL,
    [CreationDate] DATETIME NOT NULL DEFAULT getutcdate(),
    [LastModificationDate] DATETIME NULL,

    PRIMARY KEY(Id)
)