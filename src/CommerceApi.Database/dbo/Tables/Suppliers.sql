CREATE TABLE [dbo].[Suppliers] (
    [Id]                   UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [CompanyName]          NVARCHAR (100)   NOT NULL,
    [ContactName]          NVARCHAR (100)   NOT NULL,
    [City]                 NVARCHAR (50)    NOT NULL,
    [CreationDate]         DATE             NOT NULL DEFAULT getutcdate(),
    [CreationTime]         TIME(7)          NOT NULL,
    [LastModificationDate] DATE             NULL,
    [LastModificationTime] TIME(7)          NULL,

    PRIMARY KEY CLUSTERED ([Id] ASC)
);

