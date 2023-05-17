CREATE TABLE [dbo].[Categories] (
    [Id]                   UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [Name]                 NVARCHAR (100)   NOT NULL,
    [Description]          NVARCHAR (512)   NULL,
    [CreationDate]         DATETIME         DEFAULT (getutcdate()) NOT NULL,
    [LastModificationDate] DATETIME         NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

