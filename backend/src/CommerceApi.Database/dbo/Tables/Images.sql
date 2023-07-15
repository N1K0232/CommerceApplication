CREATE TABLE [dbo].[Images] (
    [Id]                   UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [FileName]             NVARCHAR (256)   NOT NULL,
    [Path]                 NVARCHAR (512)   NOT NULL,
    [Title]                NVARCHAR (100)   NOT NULL,
    [DownloadFileName]     NVARCHAR (256)   NOT NULL,
    [DownloadPath]         NVARCHAR (512)   NOT NULL,
    [ContentType]          NVARCHAR (100)   NULL,
    [Extension]            NVARCHAR (100)   NOT NULL,
    [Length]               BIGINT           NOT NULL,
    [Description]          NVARCHAR (512)   NULL,
    [SecurityStamp]        NVARCHAR (MAX)   NOT NULL,
    [ConcurrencyStamp]     NVARCHAR (MAX)   NOT NULL,
    [CreationDate]         DATE             NOT NULL DEFAULT getutcdate(),
    [CreationTime]         TIME(7)          NOT NULL, 
    [LastModificationDate] DATE             NULL,
    [LastModificationTime] TIME(7)          NULL,

    PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_FileName]
    ON [dbo].[Images]([FileName] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_DownloadFileName]
    ON [dbo].[Images]([DownloadFileName] ASC);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_DownloadPath]
    ON [dbo].[Images]([DownloadPath] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Path]
    ON [dbo].[Images]([Path] ASC);