CREATE TABLE [dbo].[tbl_Users] (
    [Id]         UNIQUEIDENTIFIER NOT NULL,
    [IdentityId] UNIQUEIDENTIFIER NULL,
    [UserName]   VARCHAR (128)    NOT NULL,
    [FileSystem] VARCHAR (16)     NOT NULL,
    [DebugLevel] VARCHAR (16)     NULL,
    [Enabled]    BIT              CONSTRAINT [DF_tbl_Users_Enabled] DEFAULT ((0)) NOT NULL,
    [Created]    DATETIME2 (7)    NOT NULL,
    [Immutable]  BIT              CONSTRAINT [DF_tbl_Users_Immutable] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_tbl_Users] PRIMARY KEY CLUSTERED ([Id] ASC)
);




















GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Users]
    ON [dbo].[tbl_Users]([Id] ASC);

