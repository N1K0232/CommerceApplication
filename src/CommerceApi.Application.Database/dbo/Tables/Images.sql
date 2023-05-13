CREATE TABLE [dbo].[Images]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [FileName] NVARCHAR(256) NOT NULL,
    [Path] NVARCHAR(512) NOT NULL,
    [Title] NVARCHAR(100) NOT NULL,
    [DownloadFileName] NVARCHAR(512) NOT NULL,
    [ContentType] NVARCHAR(100) NULL,
    [Length] BIGINT NOT NULL,
    [Description] NVARCHAR(512) NULL,
    [CreationDate] DATETIME NOT NULL DEFAULT getutcdate(),
    [LastModificationDate] DATETIME NULL,
    
    PRIMARY KEY(Id)
);

GO
CREATE UNIQUE INDEX [IX_FileName] ON [dbo].[Images]([FileName])

GO
CREATE UNIQUE INDEX [IX_Path] ON [dbo].[Images]([Path])

GO
CREATE UNIQUE INDEX [IX_DownloadFileName] ON [dbo].[Images]([DownloadFileName])