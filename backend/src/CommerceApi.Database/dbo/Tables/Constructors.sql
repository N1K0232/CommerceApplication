CREATE TABLE [dbo].[Constructors]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [Name] NVARCHAR(100) NOT NULL,
    [Street] NVARCHAR(100) NOT NULL,
    [City] NVARCHAR(100) NOT NULL,
    [PostalCode] NVARCHAR(20) NOT NULL,
    [SecurityStamp] NVARCHAR (MAX) NOT NULL,
    [ConcurrencyStamp] NVARCHAR (MAX) NOT NULL,
    [WebSiteUrl] NVARCHAR(MAX) NULL,
    [CreationDate] DATE NOT NULL DEFAULT getutcdate(),
    [CreationTime] TIME(7) NOT NULL,
    [LastModificationDate] DATE NULL,
    [LastModificationTime] TIME(7) NULL,

    PRIMARY KEY([Id])
) 