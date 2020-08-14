CREATE TABLE [dbo].[tbl_PrivateKeys] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [IdentityId]  UNIQUEIDENTIFIER NULL,
    [PublicKeyId] UNIQUEIDENTIFIER NOT NULL,
    [KeyValue]    NVARCHAR (MAX)   NOT NULL,
    [KeyAlgo]     NVARCHAR (16)    NOT NULL,
    [KeyPass]     NVARCHAR (1024)  NOT NULL,
    [KeyFormat]   NVARCHAR (16)    NOT NULL,
    [Enabled]     BIT              CONSTRAINT [DF_tbl_UserPrivateKeys_Enabled] DEFAULT ((0)) NOT NULL,
    [Created]     DATETIME2 (7)    NOT NULL,
    [LastUpdated] DATETIME2 (7)    NULL,
    [Immutable]   BIT              CONSTRAINT [DF_tbl_UserPrivateKeys_Immutable] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_tbl_UserPrivateKeys] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_PrivateKeys_IdentityID] FOREIGN KEY ([IdentityId]) REFERENCES [dbo].[tbl_Users] ([IdentityId]) ON DELETE CASCADE ON UPDATE CASCADE
);




















GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_UserPrivateKeys]
    ON [dbo].[tbl_PrivateKeys]([Id] ASC);

