CREATE TABLE [dbo].[tbl_PublicKeys] (
    [Id]           UNIQUEIDENTIFIER NOT NULL,
    [IdentityId]   UNIQUEIDENTIFIER NULL,
    [PrivateKeyId] UNIQUEIDENTIFIER NULL,
    [KeyValue]     NVARCHAR (MAX)   NOT NULL,
    [KeyAlgo]      NVARCHAR (16)    NOT NULL,
    [KeyFormat]    NVARCHAR (16)    NOT NULL,
    [SigValue]     NVARCHAR (512)   NOT NULL,
    [SigAlgo]      NVARCHAR (16)    NOT NULL,
    [Hostname]     NVARCHAR (1024)  NULL,
    [Enabled]      BIT              NOT NULL,
    [Created]      DATETIME2 (7)    NOT NULL,
    [LastUpdated]  DATETIME2 (7)    NULL,
    [Immutable]    BIT              CONSTRAINT [DF_tbl_UserPublicKeys_Immutable] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_tbl_UserPublicKeys] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_PublicKeys_IdentityID] FOREIGN KEY ([IdentityId]) REFERENCES [dbo].[tbl_Users] ([IdentityId]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_PublicKeys_PrivateKeyID] FOREIGN KEY ([PrivateKeyId]) REFERENCES [dbo].[tbl_PrivateKeys] ([Id])
);


















GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_UserPublicKeys]
    ON [dbo].[tbl_PublicKeys]([Id] ASC);

