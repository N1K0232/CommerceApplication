CREATE TABLE [dbo].[Suppliers] (
    [Id]                   UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [CompanyName]          NVARCHAR (100)   NOT NULL,
    [ContactName]          NVARCHAR (100)   NOT NULL,
    [City]                 NVARCHAR (50)    NOT NULL,
    [CreationDate]         DATETIME         DEFAULT (getutcdate()) NOT NULL,
    [LastModificationDate] DATETIME         NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

