CREATE TABLE [dbo].[AspNetUsers] (
    [Id]                         UNIQUEIDENTIFIER   DEFAULT (newid()) NOT NULL,
    [FirstName]                  NVARCHAR (256)     NOT NULL,
    [LastName]                   NVARCHAR (256)     NULL,
    [DateOfBirth]                DATE               NULL,
    [Street]                     NVARCHAR (100)     NULL,
    [City]                       NVARCHAR (50)      NULL,
    [PostalCode]                 NVARCHAR (50)      NULL,
    [Country]                    NVARCHAR (50)      NULL,
    [Email]                      NVARCHAR (256)     NOT NULL,
    [NormalizedEmail]            NVARCHAR (256)     NOT NULL,
    [UserName]                   NVARCHAR (256)     NOT NULL,
    [NormalizedUserName]         NVARCHAR (256)     NOT NULL,
    [EmailConfirmed]             BIT                NOT NULL,
    [PasswordHash]               NVARCHAR (MAX)     NOT NULL,
    [RegistrationDate]           DATETIME           DEFAULT (getutcdate()) NOT NULL,
    [Status]                     NVARCHAR (50)      NOT NULL,
    [SecurityStamp]              NVARCHAR (MAX)     NOT NULL,
    [ConcurrencyStamp]           NVARCHAR (MAX)     NOT NULL,
    [PhoneNumber]                NVARCHAR (MAX)     NULL,
    [PhoneNumberConfirmed]       BIT                NOT NULL,
    [TwoFactorEnabled]           BIT                NOT NULL,
    [LockoutEnd]                 DATETIMEOFFSET (7) NULL,
    [LockoutEnabled]             BIT                NOT NULL,
    [AccessFailedCount]          INT                NOT NULL,
    [RefreshToken]               NVARCHAR (512)     NULL,
    [RefreshTokenExpirationDate] DATETIME           NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [EmailIndex]
    ON [dbo].[AspNetUsers]([NormalizedEmail] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex]
    ON [dbo].[AspNetUsers]([NormalizedUserName] ASC) WHERE ([NormalizedUserName] IS NOT NULL);

