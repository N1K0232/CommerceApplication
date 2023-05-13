CREATE TABLE [dbo].[Invoices]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [FileName] NVARCHAR(256) NOT NULL,
    [Path] NVARCHAR(512) NOT NULL,
    [Length] BIGINT NOT NULL,
    [ContentType] NVARCHAR(100) NULL,
    [DownloadFileName] NVARCHAR(512) NOT NULL,
    [CreationDate] DATETIME NOT NULL DEFAULT getutcdate(), 
    [LastModificationDate] DATETIME NULL,

    PRIMARY KEY([Id])
);

GO
CREATE UNIQUE INDEX [IX_FileName] ON [dbo].[Invoices]([FileName])

GO
CREATE UNIQUE INDEX [IX_Path] ON [dbo].[Invoices]([Path])

GO
CREATE UNIQUE INDEX [IX_DownloadFileName] ON [dbo].[Invoices]([DownloadFileName])