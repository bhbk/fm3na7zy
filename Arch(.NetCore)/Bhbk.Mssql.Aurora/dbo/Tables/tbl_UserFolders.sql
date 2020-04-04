CREATE TABLE [dbo].[tbl_UserFolders] (
    [Id]                UNIQUEIDENTIFIER NOT NULL,
    [UserId]            UNIQUEIDENTIFIER NOT NULL,
    [VirtualParentId]   UNIQUEIDENTIFIER NULL,
    [VirtualFolderName] VARCHAR (MAX)    NOT NULL,
    [Created]           DATETIME2 (7)    NOT NULL,
    [LastAccessed]      DATETIME2 (7)    NULL,
    [LastUpdated]       DATETIME2 (7)    NULL,
    CONSTRAINT [PK_tbl_UserFolders] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_UserFolders_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_Users] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_UserFolders_VirtualParentID] FOREIGN KEY ([VirtualParentId]) REFERENCES [dbo].[tbl_UserFolders] ([Id])
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_UserFolders]
    ON [dbo].[tbl_UserFolders]([Id] ASC);

