CREATE TABLE [dbo].[Products]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [CategoryId] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [Name] NVARCHAR(100) NOT NULL,
    [Quantity] INTEGER NOT NULL,
    [Price] DECIMAL(8,2) NOT NULL,
    [DiscountPercentage] FLOAT NULL,
    [HasDiscount] BIT NOT NULL,
    [CreationDate] DATETIME NOT NULL DEFAULT getutcdate(),
    [LastModificationDate] DATETIME NULL,
    [IsDeleted] BIT NOT NULL,
    [DeletedDate] DATETIME NULL,

    PRIMARY KEY(Id),
    FOREIGN KEY(CategoryId) REFERENCES Categories(Id)
)