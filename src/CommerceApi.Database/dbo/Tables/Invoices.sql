CREATE TABLE [dbo].[Invoices] (
    [Id]                   UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [ProductId]            UNIQUEIDENTIFIER NOT NULL,
    [Price]                DECIMAL (8, 2)   NOT NULL,
    [Quantity]             INT              NOT NULL,
    [TotalPrice]           DECIMAL (8, 2)   NOT NULL,
    [CreationDate]         DATETIME         DEFAULT (getutcdate()) NOT NULL,
    [LastModificationDate] DATETIME         NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id])
);

