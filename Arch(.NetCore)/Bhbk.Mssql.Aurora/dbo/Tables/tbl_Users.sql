CREATE TABLE [dbo].[tbl_Users] (
    [Id]         UNIQUEIDENTIFIER NOT NULL,
    [IdentityId] UNIQUEIDENTIFIER NULL,
    [UserName]   VARCHAR (128)    NULL,
    [Enabled]    BIT              NULL,
    [Created]    DATETIME2 (7)    NOT NULL,
    [Immutable]  BIT              NOT NULL,
    CONSTRAINT [PK_tbl_Users] PRIMARY KEY CLUSTERED ([Id] ASC)
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Users]
    ON [dbo].[tbl_Users]([Id] ASC);

