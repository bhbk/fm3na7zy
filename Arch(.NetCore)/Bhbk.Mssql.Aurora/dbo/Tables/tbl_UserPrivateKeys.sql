CREATE TABLE [dbo].[tbl_UserPrivateKeys] (
    [Id]             UNIQUEIDENTIFIER NOT NULL,
    [UserId]         UNIQUEIDENTIFIER NOT NULL,
    [PublicKeyId]    UNIQUEIDENTIFIER NOT NULL,
    [KeyValueBase64] NVARCHAR (MAX)   NOT NULL,
    [KeyValueAlgo]   NVARCHAR (16)    NOT NULL,
    [KeyValuePass]   NVARCHAR (1024)  NOT NULL,
    [Enabled]        BIT              CONSTRAINT [DF_tbl_UserPrivateKeys_Enabled] DEFAULT ((0)) NOT NULL,
    [Created]        DATETIME2 (7)    NOT NULL,
    [Immutable]      BIT              CONSTRAINT [DF_tbl_UserPrivateKeys_Immutable] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_tbl_UserPrivateKeys] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_UserPrivateKeys_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_Users] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);














GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_UserPrivateKeys]
    ON [dbo].[tbl_UserPrivateKeys]([Id] ASC);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_UserPrivateKeys_PublicKeyID]
    ON [dbo].[tbl_UserPrivateKeys]([PublicKeyId] ASC);

