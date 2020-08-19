CREATE TABLE [dbo].[tbl_Credential] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [Domain]      VARCHAR (128)    NULL,
    [UserName]    VARCHAR (128)    NOT NULL,
    [Password]    VARCHAR (128)    NOT NULL,
    [Enabled]     BIT              NOT NULL,
    [Deletable]   BIT              NOT NULL,
    [Created]     DATETIME2 (7)    NOT NULL,
    [LastUpdated] DATETIME2 (7)    NULL,
    CONSTRAINT [PK_tbl_SysCredential] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_SysCredentials]
    ON [dbo].[tbl_Credential]([Id] ASC);

