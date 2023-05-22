CREATE TABLE [dbo].[Invoices] (
    [Id]                   UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [ProductId]            UNIQUEIDENTIFIER NOT NULL,
    [Price]                DECIMAL (8, 2)   NOT NULL,
    [Quantity]             INT              NOT NULL,
    [TotalPrice]           DECIMAL (8, 2)   NOT NULL,
    [CreationDate]         DATE             NOT NULL DEFAULT getutcdate(),
    [CreationTime]         TIME(7)          NOT NULL,
    [LastModificationDate] DATE             NULL,
    [LastModificationTime] TIME(7)          NULL,

    PRIMARY KEY CLUSTERED ([Id] ASC),
    FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id])
);

