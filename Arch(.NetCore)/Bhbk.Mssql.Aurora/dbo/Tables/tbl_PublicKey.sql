CREATE TABLE [dbo].[tbl_PublicKey] (
    [Id]           UNIQUEIDENTIFIER NOT NULL,
    [IdentityId]   UNIQUEIDENTIFIER NULL,
    [PrivateKeyId] UNIQUEIDENTIFIER NULL,
    [KeyValue]     NVARCHAR (MAX)   NOT NULL,
    [KeyAlgo]      NVARCHAR (16)    NOT NULL,
    [KeyFormat]    NVARCHAR (16)    NOT NULL,
    [SigValue]     NVARCHAR (512)   NOT NULL,
    [SigAlgo]      NVARCHAR (16)    NOT NULL,
    [Comment]      NVARCHAR (1024)  NULL,
    [Enabled]      BIT              NOT NULL,
    [Deletable]    BIT              CONSTRAINT [DF_tbl_UserPublicKeys_Deletable] DEFAULT ((0)) NOT NULL,
    [Created]      DATETIME2 (7)    NOT NULL,
    [LastUpdated]  DATETIME2 (7)    NULL,
    CONSTRAINT [PK_tbl_PublicKey] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_PublicKey_IdentityID] FOREIGN KEY ([IdentityId]) REFERENCES [dbo].[tbl_User] ([IdentityId]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_PublicKey_PrivateKeyID] FOREIGN KEY ([PrivateKeyId]) REFERENCES [dbo].[tbl_PrivateKey] ([Id])
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_PublicKey]
    ON [dbo].[tbl_PublicKey]([Id] ASC);

