CREATE TABLE [dbo].[tbl_PublicKey] (
    [Id]             UNIQUEIDENTIFIER   NOT NULL,
    [UserId]         UNIQUEIDENTIFIER   NULL,
    [PrivateKeyId]   UNIQUEIDENTIFIER   NULL,
    [KeyValue]       NVARCHAR (MAX)     NOT NULL,
    [KeyAlgorithmId] INT                NOT NULL,
    [KeyFormatId]    INT                NOT NULL,
    [SigValue]       NVARCHAR (512)     NOT NULL,
    [SigAlgorithmId] INT                NOT NULL,
    [Comment]        NVARCHAR (1024)    NULL,
    [IsEnabled]      BIT                NOT NULL,
    [IsDeletable]    BIT                NOT NULL,
    [CreatedUtc]     DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_tbl_PublicKey] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_PublicKey_tbl_KeyAlgorithmType] FOREIGN KEY ([KeyAlgorithmId]) REFERENCES [dbo].[tbl_KeyAlgorithmType] ([Id]) ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_PublicKey_tbl_Login] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_Login] ([UserId]) ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_PublicKey_tbl_PublicKeyFormatType] FOREIGN KEY ([KeyFormatId]) REFERENCES [dbo].[tbl_PublicKeyFormatType] ([Id]) ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_PublicKey_tbl_PublicKeySignatureType] FOREIGN KEY ([SigAlgorithmId]) REFERENCES [dbo].[tbl_PublicKeySignatureType] ([Id]) ON UPDATE CASCADE
);














GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_PublicKey]
    ON [dbo].[tbl_PublicKey]([Id] ASC);

