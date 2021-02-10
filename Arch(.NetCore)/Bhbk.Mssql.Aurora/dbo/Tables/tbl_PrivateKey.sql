CREATE TABLE [dbo].[tbl_PrivateKey] (
    [Id]             UNIQUEIDENTIFIER   NOT NULL,
    [UserId]         UNIQUEIDENTIFIER   NULL,
    [PublicKeyId]    UNIQUEIDENTIFIER   NOT NULL,
    [KeyValue]       NVARCHAR (MAX)     NOT NULL,
    [KeyAlgorithmId] INT                NOT NULL,
    [KeyFormatId]    INT                NOT NULL,
    [EncryptedPass]  NVARCHAR (1024)    NOT NULL,
    [IsEnabled]      BIT                NOT NULL,
    [IsDeletable]    BIT                NOT NULL,
    [CreatedUtc]     DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_tbl_PrivateKey] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_PrivateKey_tbl_KeyAlgorithmType] FOREIGN KEY ([KeyAlgorithmId]) REFERENCES [dbo].[tbl_KeyAlgorithmType] ([Id]) ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_PrivateKey_tbl_Login] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_Login] ([UserId]) ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_PrivateKey_tbl_PrivateKeyFormatType] FOREIGN KEY ([KeyFormatId]) REFERENCES [dbo].[tbl_PrivateKeyFormatType] ([Id]) ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_PrivateKey_tbl_PublicKey] FOREIGN KEY ([PublicKeyId]) REFERENCES [dbo].[tbl_PublicKey] ([Id])
);
















GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_PrivateKey]
    ON [dbo].[tbl_PrivateKey]([Id] ASC);

