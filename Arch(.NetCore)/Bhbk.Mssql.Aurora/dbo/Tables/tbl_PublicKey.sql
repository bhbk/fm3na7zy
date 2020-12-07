CREATE TABLE [dbo].[tbl_PublicKey] (
    [Id]             UNIQUEIDENTIFIER   NOT NULL,
    [IdentityId]     UNIQUEIDENTIFIER   NULL,
    [PrivateKeyId]   UNIQUEIDENTIFIER   NULL,
    [KeyValue]       NVARCHAR (MAX)     NOT NULL,
    [KeyAlgo]        NVARCHAR (16)      NOT NULL,
    [KeyFormat]      NVARCHAR (16)      NOT NULL,
    [SigValue]       NVARCHAR (512)     NOT NULL,
    [SigAlgo]        NVARCHAR (16)      NOT NULL,
    [Comment]        NVARCHAR (1024)    NULL,
    [IsEnabled]      BIT                NOT NULL,
    [IsDeletable]    BIT                NOT NULL,
    [CreatedUtc]     DATETIMEOFFSET (7) NOT NULL,
    [LastUpdatedUtc] DATETIMEOFFSET (7) NULL,
    CONSTRAINT [PK_tbl_PublicKey] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_PublicKey_IdentityID] FOREIGN KEY ([IdentityId]) REFERENCES [dbo].[tbl_UserLogin] ([IdentityId]) ON UPDATE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_PublicKey]
    ON [dbo].[tbl_PublicKey]([Id] ASC);

