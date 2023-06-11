CREATE TABLE [dbo].[DataProtectionKeys]
(
	[Id] INT NOT NULL IDENTITY(1, 1),
    [FriendlyName] NVARCHAR(MAX) NULL,
    [Xml] NVARCHAR(MAX) NULL,

    PRIMARY KEY([Id])
)