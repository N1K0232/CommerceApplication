CREATE TABLE [dbo].[ContentPages]
(
	[Id] UNIQUEIDENTIFIER NOT NULL,
    [Title] NVARCHAR(256) NOT NULL,
    [Url] NVARCHAR(256) NOT NULL,
    [Content] NVARCHAR(MAX) NOT NULL, 
    [IsPublished] BIT NOT NULL,
    [CreationDate] DATETIME NOT NULL,
    [UpdatedDate] DATETIME NULL,
)