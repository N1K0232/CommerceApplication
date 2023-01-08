CREATE TABLE [dbo].[Images]
(
	[Id] UNIQUEIDENTIFIER NOT NULL,
    [FileName] NVARCHAR(256) NOT NULL,
    [Path] NVARCHAR(512) NOT NULL,
    [ContentType] NVARCHAR(100) NOT NULL,
    [Length] BIGINT NOT NULL,
    [Description] NVARCHAR(512) NULL,
    [CreationDate] DATETIME NOT NULL,
    [UpdatedDate] DATETIME NULL,
    
    PRIMARY KEY(Id)
)