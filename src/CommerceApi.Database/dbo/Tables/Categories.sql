CREATE TABLE [dbo].[Categories] (
    [Id]                   UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [Name]                 NVARCHAR (100)   NOT NULL,
    [Description]          NVARCHAR (512)   NULL,
    [SecurityStamp]        NVARCHAR (MAX)   NOT NULL,
    [ConcurrencyStamp]     NVARCHAR (MAX)   NOT NULL,
    [CreationDate]         DATE             NOT NULL DEFAULT getutcdate(),
    [CreationTime]         TIME(7)          NOT NULL,
    [LastModificationDate] DATE             NULL,
    [LastModificationTime] TIME(7)          NULL,

    PRIMARY KEY CLUSTERED ([Id] ASC)
);

