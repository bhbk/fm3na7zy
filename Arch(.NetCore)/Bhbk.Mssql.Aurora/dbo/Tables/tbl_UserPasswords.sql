CREATE TABLE [dbo].[tbl_UserPasswords] (
    [UserId]           UNIQUEIDENTIFIER NOT NULL,
    [ConcurrencyStamp] NVARCHAR (1024)  NOT NULL,
    [HashPBKDF2]       NVARCHAR (2048)  NULL,
    [HashSHA256]       NVARCHAR (2048)  NULL,
    [SecurityStamp]    NVARCHAR (1024)  NOT NULL,
    [Enabled]          BIT              CONSTRAINT [DF_tbl_UserPasswords_Enabled] DEFAULT ((0)) NOT NULL,
    [Created]          DATETIME2 (7)    NOT NULL,
    [Immutable]        BIT              CONSTRAINT [DF_tbl_UserPasswords_Immutable] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_tbl_UserPasswords] PRIMARY KEY CLUSTERED ([UserId] ASC),
    CONSTRAINT [FK_tbl_UserPasswords_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_Users] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);










GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_UserPasswords]
    ON [dbo].[tbl_UserPasswords]([UserId] ASC);

