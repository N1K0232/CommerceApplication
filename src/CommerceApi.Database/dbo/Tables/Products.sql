CREATE TABLE [dbo].[Products] (
    [Id]                   UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [CategoryId]           UNIQUEIDENTIFIER NOT NULL,
    [ConstructorId]        UNIQUEIDENTIFIER NOT NULL,
    [SupplierId]           UNIQUEIDENTIFIER NOT NULL,
    [Name]                 NVARCHAR (256)   NOT NULL,
    [Description]          NVARCHAR (4000)  NOT NULL,
    [IdentificationCode]   NVARCHAR (100)   NOT NULL,
    [Quantity]             INT              NOT NULL,
    [Price]                DECIMAL (8, 2)   NOT NULL,
    [DiscountPercentage]   FLOAT (53)       NULL,
    [HasDiscount]          BIT              NOT NULL,
    [ShippingCost]         DECIMAL(5, 2)    NULL,
    [HasShipping]          BIT              NOT NULL,
    [AverageScore]         FLOAT            NULL,
    [IsPublished]          BIT              NOT NULL,
    [IsAvailable]          BIT              NOT NULL,
    [SecurityStamp]        NVARCHAR (MAX)   NOT NULL,
    [ConcurrencyStamp]     NVARCHAR (MAX)   NOT NULL,
    [CreationDate]         DATE         NOT NULL DEFAULT getutcdate(),
    [CreationTime]         TIME(7)      NOT NULL,
    [LastModificationDate] DATE         NULL,
    [LastModificationTime] TIME(7)      NULL,
    [IsDeleted]            BIT          NOT NULL,
    [DeletedDate]          DATE         NULL,
    [DeletedTime]          TIME(7)      NULL,

    PRIMARY KEY CLUSTERED ([Id] ASC),
    FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[Categories] ([Id]),
    FOREIGN KEY ([ConstructorId]) REFERENCES [dbo].[Constructors] ([Id]),
    FOREIGN KEY ([SupplierId]) REFERENCES [dbo].[Suppliers] ([Id])
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_ProductName]
    ON [dbo].[Products]([Name] ASC)

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_ProductCode]
    ON [dbo].[Products]([IdentificationCode] ASC)