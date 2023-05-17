CREATE TABLE [dbo].[Images] (
    [Id]                   UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [FileName]             NVARCHAR (256)   NOT NULL,
    [Path]                 NVARCHAR (512)   NOT NULL,
    [Title]                NVARCHAR (100)   NOT NULL,
    [DownloadFileName]     NVARCHAR (512)   NOT NULL,
    [ContentType]          NVARCHAR (100)   NULL,
    [Length]               BIGINT           NOT NULL,
    [Description]          NVARCHAR (512)   NULL,
    [CreationDate]         DATETIME         DEFAULT (getutcdate()) NOT NULL,
    [LastModificationDate] DATETIME         NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_FileName]
    ON [dbo].[Images]([FileName] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_DownloadFileName]
    ON [dbo].[Images]([DownloadFileName] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Path]
    ON [dbo].[Images]([Path] ASC);

