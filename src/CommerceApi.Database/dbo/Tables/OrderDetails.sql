CREATE TABLE [dbo].[OrderDetails]
(
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [OrderId] UNIQUEIDENTIFIER NOT NULL,
    [ProductId] UNIQUEIDENTIFIER NOT NULL,
    [UnitPrice] DECIMAL(8,2) NOT NULL,
    [Quantity] INTEGER NOT NULL,
    [CreationDate] DATE NOT NULL DEFAULT getutcdate(),
    [CreationTime] TIME(7) NOT NULL,
    [LastModificationDate] DATE NULL,
    [LastModificationTime] TIME(7) NULL,
    [IsDeleted] BIT NOT NULL,
    [DeletedDate] DATE NULL,
    [DeletedTime] TIME(7) NULL,

    PRIMARY KEY([Id]),
    FOREIGN KEY([OrderId]) REFERENCES [dbo].[Orders]([Id]),
    FOREIGN KEY([ProductId]) REFERENCES [dbo].[Products]([Id])
)