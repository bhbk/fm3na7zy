CREATE TABLE [dbo].[tbl_UserFolders] (
    [Id]           UNIQUEIDENTIFIER NOT NULL,
    [UserId]       UNIQUEIDENTIFIER NOT NULL,
    [ParentId]     UNIQUEIDENTIFIER NULL,
    [VirtualName]  VARCHAR (MAX)    NOT NULL,
    [ReadOnly]     BIT              CONSTRAINT [DF_tbl_UserFolders_ReadOnly] DEFAULT ((0)) NOT NULL,
    [Created]      DATETIME2 (7)    NOT NULL,
    [LastAccessed] DATETIME2 (7)    NULL,
    [LastUpdated]  DATETIME2 (7)    NULL,
    CONSTRAINT [PK_tbl_UserFolders] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_UserFolders_ParentID] FOREIGN KEY ([ParentId]) REFERENCES [dbo].[tbl_UserFolders] ([Id]),
    CONSTRAINT [FK_tbl_UserFolders_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_Users] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);












GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_UserFolders]
    ON [dbo].[tbl_UserFolders]([Id] ASC);

