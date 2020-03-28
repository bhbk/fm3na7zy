CREATE TABLE [dbo].[tbl_UserPublicKeys] (
    [Id]             UNIQUEIDENTIFIER NOT NULL,
    [UserId]         UNIQUEIDENTIFIER NOT NULL,
    [PrivateKeyId]   UNIQUEIDENTIFIER NULL,
    [KeyValueBase64] NVARCHAR (2048)  NOT NULL,
    [KeyValueAlgo]   NVARCHAR (16)    NOT NULL,
    [KeySig]         NVARCHAR (512)   NOT NULL,
    [KeySigAlgo]     NVARCHAR (16)    NOT NULL,
    [Hostname]       NVARCHAR (1024)  NOT NULL,
    [Enabled]        BIT              NOT NULL,
    [Created]        DATETIME2 (7)    NOT NULL,
    CONSTRAINT [PK_tbl_UserPublicKeys] PRIMARY KEY CLUSTERED ([Id] ASC, [UserId] ASC),
    CONSTRAINT [FK_tbl_UserPublicKeys_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_Users] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);










GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_UserPublicKeys]
    ON [dbo].[tbl_UserPublicKeys]([Id] ASC, [UserId] ASC);

