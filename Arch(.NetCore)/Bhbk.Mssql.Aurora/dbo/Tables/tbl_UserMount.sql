﻿CREATE TABLE [dbo].[tbl_UserMount] (
    [IdentityId]    UNIQUEIDENTIFIER NOT NULL,
    [CredentialId]  UNIQUEIDENTIFIER NULL,
    [AuthType]      VARCHAR (16)     NOT NULL,
    [ServerAddress] VARCHAR (256)    NOT NULL,
    [ServerShare]   VARCHAR (256)    NOT NULL,
    [Enabled]       BIT              NOT NULL,
    [Created]       DATETIME2 (7)    NOT NULL,
    [LastUpdated]   DATETIME2 (7)    NULL,
    [Deletable]     BIT              NOT NULL,
    CONSTRAINT [PK_tbl_UserMount] PRIMARY KEY CLUSTERED ([IdentityId] ASC),
    CONSTRAINT [FK_tbl_UserMount_CredentialID] FOREIGN KEY ([CredentialId]) REFERENCES [dbo].[tbl_Credential] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_UserMount_IdentityID] FOREIGN KEY ([IdentityId]) REFERENCES [dbo].[tbl_User] ([IdentityId]) ON DELETE CASCADE ON UPDATE CASCADE
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_UserMounts]
    ON [dbo].[tbl_UserMount]([IdentityId] ASC);

