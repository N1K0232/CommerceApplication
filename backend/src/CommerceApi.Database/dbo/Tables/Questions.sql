CREATE TABLE [dbo].[Questions]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [ProductId] UNIQUEIDENTIFIER NOT NULL,
    [Text] NVARCHAR(4000) NOT NULL,
    [Date] DATETIME NOT NULL DEFAULT getutcdate(),
    [IsPublished] BIT NOT NULL,
    [CreationDate] DATE NOT NULL DEFAULT getutcdate(),
    [CreationTime] TIME(7) NOT NULL,
    [LastModificationDate] DATE NULL,
    [LastModificationTime] TIME(7) NULL,

    PRIMARY KEY([Id]),
    FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]),
    FOREIGN KEY([ProductId]) REFERENCES [dbo].[Products]([Id])
)