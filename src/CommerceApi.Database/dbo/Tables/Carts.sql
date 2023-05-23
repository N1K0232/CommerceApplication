CREATE TABLE [dbo].[Carts]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [CreationDate] DATE NOT NULL DEFAULT getutcdate(),
    [CreationTime] TIME(7) NOT NULL,
    [LastModificationDate] DATE NULL,
    [LastModificationTime] TIME(7) NULL,
    [IsDeleted] BIT NOT NULL,
    [DeletedDate] DATE NULL,
    [DeletedTime] TIME NULL,

    PRIMARY KEY([Id]),
    FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers]([Id])
)