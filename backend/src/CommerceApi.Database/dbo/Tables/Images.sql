CREATE TABLE [dbo].[Images] (
    [Id]                   UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [Title]                NVARCHAR (100)   NOT NULL,
    [DownloadFileName]     NVARCHAR (512)   NOT NULL,
    [ContentType]          NVARCHAR (25)    NULL,
    [Extension]            NVARCHAR (25)    NOT NULL,
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
CREATE UNIQUE NONCLUSTERED INDEX [IX_DownloadFileName]
    ON [dbo].[Images]([DownloadFileName] ASC);