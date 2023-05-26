CREATE TABLE [dbo].[CartItems]
(
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [CartId] UNIQUEIDENTIFIER NOT NULL,
    [ProductId] UNIQUEIDENTIFIER NOT NULL,
    [UnitPrice] DECIMAL(8,2) NOT NULL,
    [Quantity] INTEGER NOT NULL,
    [ConcurrencyStamp] NVARCHAR(50) NOT NULL,
    [CreationDate] DATE NOT NULL DEFAULT getutcdate(),
    [CreationTime] TIME(7) NOT NULL,
    [LastModificationDate] DATE NULL,
    [LastModificationTime] TIME(7) NULL,
    [IsDeleted] BIT NOT NULL,
    [DeletedDate] DATE NULL,
    [DeletedTime] TIME NULL,

    PRIMARY KEY([Id]),
    FOREIGN KEY([CartId]) REFERENCES [dbo].[Carts]([Id]),
    FOREIGN KEY([ProductId]) REFERENCES [dbo].[Products]([Id])
)