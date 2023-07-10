CREATE TABLE [dbo].[Tenants]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [Name] NVARCHAR(256) NOT NULL,
    [ConnectionString] NVARCHAR(4000) NOT NULL,
    [StorageConnectionString] NVARCHAR(4000) NULL,
    [ContainerName] NVARCHAR(256) NULL,

    PRIMARY KEY([Id])
)

GO
CREATE NONCLUSTERED INDEX [IX_Name] ON [dbo].[Tenants]([Name])