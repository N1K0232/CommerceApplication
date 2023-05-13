CREATE TABLE [dbo].[Invoices]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [ProductId] UNIQUEIDENTIFIER NOT NULL,
    [Price] DECIMAL(8,2) NOT NULL,
    [Quantity] INTEGER NOT NULL,
    [TotalPrice] DECIMAL(8,2) NOT NULL,
    [CreationDate] DATETIME NOT NULL DEFAULT getutcdate(), 
    [LastModificationDate] DATETIME NULL,

    PRIMARY KEY([Id]),
    FOREIGN KEY([ProductId]) REFERENCES Products([Id])
);