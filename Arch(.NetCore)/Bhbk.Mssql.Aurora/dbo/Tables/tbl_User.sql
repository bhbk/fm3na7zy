CREATE TABLE [dbo].[tbl_User] (
    [IdentityId]         UNIQUEIDENTIFIER   NOT NULL,
    [IdentityAlias]      VARCHAR (128)      NOT NULL,
    [RequirePassword]    BIT                CONSTRAINT [DF_tbl_User_RequirePassword] DEFAULT ((0)) NOT NULL,
    [RequirePublicKey]   BIT                CONSTRAINT [DF_tbl_User_RequirePublicKey] DEFAULT ((0)) NOT NULL,
    [FileSystemType]     VARCHAR (16)       NOT NULL,
    [FileSystemReadOnly] BIT                CONSTRAINT [DF_tbl_User_FileSystemReadOnly] DEFAULT ((0)) NOT NULL,
    [DebugLevel]         VARCHAR (16)       NULL,
    [IsEnabled]          BIT                NOT NULL,
    [IsDeletable]        BIT                NOT NULL,
    [CreatedUtc]         DATETIMEOFFSET (7) NOT NULL,
    [LastUpdatedUtc]     DATETIMEOFFSET (7) NULL,
    CONSTRAINT [PK_tbl_Users] PRIMARY KEY CLUSTERED ([IdentityId] ASC)
);












GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Users]
    ON [dbo].[tbl_User]([IdentityId] ASC);

