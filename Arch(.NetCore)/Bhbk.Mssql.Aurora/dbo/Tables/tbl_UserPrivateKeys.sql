﻿CREATE TABLE [dbo].[tbl_UserPrivateKeys] (
    [Id]             UNIQUEIDENTIFIER NOT NULL,
    [UserId]         UNIQUEIDENTIFIER NOT NULL,
    [PublicKeyId]    UNIQUEIDENTIFIER NULL,
    [KeyValueBase64] NVARCHAR (2048)  NOT NULL,
    [KeyValueAlgo]   NVARCHAR (16)    NOT NULL,
    [KeyValuePass]   NVARCHAR (1024)  NOT NULL,
    [Enabled]        BIT              NOT NULL,
    [Created]        DATETIME2 (7)    NOT NULL,
    CONSTRAINT [PK_tbl_UserPrivateKeys] PRIMARY KEY CLUSTERED ([Id] ASC, [UserId] ASC),
    CONSTRAINT [FK_tbl_UserPrivateKeys_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_Users] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);






GO


