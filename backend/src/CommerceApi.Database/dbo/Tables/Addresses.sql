CREATE TABLE [dbo].[Addresses]
(
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [Street] NVARCHAR(100) NOT NULL,
    [City] NVARCHAR(50) NOT NULL,
    [PostalCode] NVARCHAR(50) NOT NULL,
    [Country] NVARCHAR(50) NOT NULL,

    PRIMARY KEY([Id]),
    FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers]([Id])
)