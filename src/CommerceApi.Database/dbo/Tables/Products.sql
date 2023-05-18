CREATE TABLE [dbo].[Products] (
    [Id]                   UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [CategoryId]           UNIQUEIDENTIFIER NOT NULL,
    [SupplierId]           UNIQUEIDENTIFIER NOT NULL,
    [Name]                 NVARCHAR (100)   NOT NULL,
    [Quantity]             INT              NOT NULL,
    [Price]                DECIMAL (8, 2)   NOT NULL,
    [DiscountPercentage]   FLOAT (53)       NULL,
    [HasDiscount]          BIT              NOT NULL,
    [CreationDate]         DATETIME         DEFAULT getutcdate() NOT NULL,
    [LastModificationDate] DATETIME         NULL,
    [IsDeleted]            BIT              NOT NULL,
    [DeletedDate]          DATETIME         NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[Categories] ([Id]),
    FOREIGN KEY ([SupplierId]) REFERENCES [dbo].[Suppliers] ([Id])
);

