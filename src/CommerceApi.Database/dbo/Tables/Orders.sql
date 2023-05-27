CREATE TABLE [dbo].[Orders]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [Status] NVARCHAR(20) NOT NULL,
    [Date] DATE NOT NULL DEFAULT getutcdate(),
    [Time] TIME(7) NOT NULL,
    [SecurityStamp] NVARCHAR(MAX) NOT NULL,
    [ConcurrencyStamp] NVARCHAR(MAX) NOT NULL,
    [CreationDate] DATE NOT NULL DEFAULT getutcdate(),
    [CreationTime] TIME(7) NOT NULL,
    [LastModificationDate] DATE NULL,
    [LastModificationTime] TIME(7) NULL,
    [IsDeleted] BIT NOT NULL,
    [DeletedDate] DATE NULL,
    [DeletedTime] TIME(7) NULL,

    PRIMARY KEY([Id]),
    FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers]([Id])
)