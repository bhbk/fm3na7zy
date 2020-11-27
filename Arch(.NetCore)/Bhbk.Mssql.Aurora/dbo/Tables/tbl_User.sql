CREATE TABLE [dbo].[tbl_User] (
    [IdentityId]           UNIQUEIDENTIFIER   NOT NULL,
    [IdentityAlias]        VARCHAR (128)      NOT NULL,
    [FileSystemType]       VARCHAR (16)       NOT NULL,
    [IsPasswordRequired]   BIT                CONSTRAINT [DF_tbl_User_IsPasswordRequired] DEFAULT ((0)) NOT NULL,
    [IsPublicKeyRequired]  BIT                CONSTRAINT [DF_tbl_User_IsPublicKeyRequired] DEFAULT ((0)) NOT NULL,
    [IsFileSystemReadOnly] BIT                CONSTRAINT [DF_tbl_User_IsFileSystemReadOnly] DEFAULT ((0)) NOT NULL,
    [QuotaInBytes]         BIGINT             NOT NULL,
    [QuotaUsedInBytes]     BIGINT             NOT NULL,
    [Debugger]             VARCHAR (16)       NULL,
    [IsEnabled]            BIT                NOT NULL,
    [IsDeletable]          BIT                NOT NULL,
    [CreatedUtc]           DATETIMEOFFSET (7) NOT NULL,
    [LastUpdatedUtc]       DATETIMEOFFSET (7) NULL,
    CONSTRAINT [PK_tbl_Users] PRIMARY KEY CLUSTERED ([IdentityId] ASC)
);






















GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Users]
    ON [dbo].[tbl_User]([IdentityId] ASC);

