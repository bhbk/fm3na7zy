CREATE TABLE [dbo].[tbl_User] (
    [IdentityId]         UNIQUEIDENTIFIER NOT NULL,
    [IdentityAlias]      VARCHAR (128)    NOT NULL,
    [RequirePassword]    BIT              CONSTRAINT [DF_tbl_User_RequirePassword] DEFAULT ((0)) NOT NULL,
    [RequirePublicKey]   BIT              CONSTRAINT [DF_tbl_User_RequirePublicKey] DEFAULT ((0)) NOT NULL,
    [FileSystemType]     VARCHAR (16)     NOT NULL,
    [FileSystemReadOnly] BIT              CONSTRAINT [DF_tbl_User_FileSystemReadOnly] DEFAULT ((0)) NOT NULL,
    [DebugLevel]         VARCHAR (16)     NULL,
    [Enabled]            BIT              CONSTRAINT [DF_tbl_User_Enabled] DEFAULT ((0)) NOT NULL,
    [Deletable]          BIT              CONSTRAINT [DF_tbl_User_Deletable] DEFAULT ((0)) NOT NULL,
    [Created]            DATETIME2 (7)    NOT NULL,
    [LastUpdated]        DATETIME2 (7)    NULL,
    CONSTRAINT [PK_tbl_Users] PRIMARY KEY CLUSTERED ([IdentityId] ASC)
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Users]
    ON [dbo].[tbl_User]([IdentityId] ASC);

