CREATE TABLE [dbo].[tbl_UserFile] (
    [Id]           UNIQUEIDENTIFIER NOT NULL,
    [IdentityId]   UNIQUEIDENTIFIER NOT NULL,
    [FolderId]     UNIQUEIDENTIFIER NULL,
    [VirtualName]  NVARCHAR (MAX)   NOT NULL,
    [ReadOnly]     BIT              CONSTRAINT [DF_tbl_UserFile_FileReadOnly] DEFAULT ((0)) NOT NULL,
    [RealPath]     NVARCHAR (MAX)   NOT NULL,
    [RealFileName] NVARCHAR (MAX)   NOT NULL,
    [RealFileSize] BIGINT           NOT NULL,
    [HashSHA256]   NVARCHAR (64)    NULL,
    [Created]      DATETIME2 (7)    NOT NULL,
    [LastAccessed] DATETIME2 (7)    NULL,
    [LastUpdated]  DATETIME2 (7)    NULL,
    [LastVerified] DATETIME2 (7)    NOT NULL,
    CONSTRAINT [PK_tbl_UserFile] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_UserFile_FolderID] FOREIGN KEY ([FolderId]) REFERENCES [dbo].[tbl_UserFolder] ([Id]),
    CONSTRAINT [FK_tbl_UserFile_IdentityID] FOREIGN KEY ([IdentityId]) REFERENCES [dbo].[tbl_User] ([IdentityId]) ON DELETE CASCADE ON UPDATE CASCADE
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_UserFiles]
    ON [dbo].[tbl_UserFile]([Id] ASC);

