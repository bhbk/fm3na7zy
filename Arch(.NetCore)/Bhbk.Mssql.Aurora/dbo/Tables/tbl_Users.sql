CREATE TABLE [dbo].[tbl_Users] (
    [IdentityId]         UNIQUEIDENTIFIER NOT NULL,
    [IdentityAlias]      VARCHAR (128)    NOT NULL,
    [AllowPassword]      BIT              NOT NULL,
    [FileSystemType]     VARCHAR (16)     NOT NULL,
    [FileSystemReadOnly] BIT              CONSTRAINT [DF_tbl_Users_FileSystemReadOnly] DEFAULT ((0)) NOT NULL,
    [DebugLevel]         VARCHAR (16)     NULL,
    [Enabled]            BIT              CONSTRAINT [DF_tbl_Users_Enabled] DEFAULT ((0)) NOT NULL,
    [Created]            DATETIME2 (7)    NOT NULL,
    [Immutable]          BIT              CONSTRAINT [DF_tbl_Users_Immutable] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_tbl_Users] PRIMARY KEY CLUSTERED ([IdentityId] ASC)
);










































GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Users]
    ON [dbo].[tbl_Users]([IdentityId] ASC);



