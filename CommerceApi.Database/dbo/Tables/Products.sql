CREATE TABLE [dbo].[Products]
(
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [CategoryId] UNIQUEIDENTIFIER NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(2000) NOT NULL,
    [Brand] NVARCHAR(100) NOT NULL,
    [Model] NVARCHAR(100) NOT NULL,
    [Condition] NVARCHAR(10) NOT NULL,
    [Status] NVARCHAR(10) NOT NULL,
    [Quantity] INTEGER NOT NULL,
    [Price] DECIMAL(5,2) NOT NULL,
    [CreationDate] DATETIME NOT NULL,
    [UpdatedDate] DATETIME NULL,
    [IsDeleted] BIT NOT NULL,
    [DeletedDate] DATETIME NULL,

    PRIMARY KEY(Id),
    FOREIGN KEY(CategoryId) REFERENCES Categories(Id)
)