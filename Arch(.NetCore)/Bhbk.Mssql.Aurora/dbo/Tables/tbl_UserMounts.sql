CREATE TABLE [dbo].[tbl_UserMounts] (
    [UserId]        UNIQUEIDENTIFIER NOT NULL,
    [CredentialId]  UNIQUEIDENTIFIER NULL,
    [AuthType]      VARCHAR (16)     NOT NULL,
    [ServerAddress] VARCHAR (256)    NOT NULL,
    [ServerShare]   VARCHAR (256)    NOT NULL,
    [Enabled]       BIT              NOT NULL,
    [Created]       DATETIME2 (7)    NOT NULL,
    [Immutable]     BIT              NOT NULL,
    CONSTRAINT [PK_tbl_UserMounts] PRIMARY KEY CLUSTERED ([UserId] ASC),
    CONSTRAINT [FK_tbl_UserMounts_CredentialID] FOREIGN KEY ([CredentialId]) REFERENCES [dbo].[tbl_Credentials] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_UserMounts_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_Users] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);






















GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_UserMounts]
    ON [dbo].[tbl_UserMounts]([UserId] ASC);



