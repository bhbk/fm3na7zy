CREATE TABLE [dbo].[tbl_PrivateKey] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [IdentityId]  UNIQUEIDENTIFIER NULL,
    [PublicKeyId] UNIQUEIDENTIFIER NOT NULL,
    [KeyValue]    NVARCHAR (MAX)   NOT NULL,
    [KeyAlgo]     NVARCHAR (16)    NOT NULL,
    [KeyPass]     NVARCHAR (1024)  NOT NULL,
    [KeyFormat]   NVARCHAR (16)    NOT NULL,
    [Enabled]     BIT              CONSTRAINT [DF_tbl_PrivateKey_Enabled] DEFAULT ((0)) NOT NULL,
    [Deletable]   BIT              CONSTRAINT [DF_tbl_PrivateKey_Deletable] DEFAULT ((0)) NOT NULL,
    [Created]     DATETIME2 (7)    NOT NULL,
    [LastUpdated] DATETIME2 (7)    NULL,
    CONSTRAINT [PK_tbl_PrivateKey] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_PrivateKey_IdentityID] FOREIGN KEY ([IdentityId]) REFERENCES [dbo].[tbl_User] ([IdentityId]) ON DELETE CASCADE ON UPDATE CASCADE
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_UserPrivateKeys]
    ON [dbo].[tbl_PrivateKey]([Id] ASC);

