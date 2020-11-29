CREATE TABLE [dbo].[tbl_UserFile] (
    [Id]              UNIQUEIDENTIFIER   NOT NULL,
    [IdentityId]      UNIQUEIDENTIFIER   NOT NULL,
    [FolderId]        UNIQUEIDENTIFIER   NOT NULL,
    [VirtualName]     NVARCHAR (260)     NOT NULL,
    [RealPath]        NVARCHAR (MAX)     NOT NULL,
    [RealFileName]    NVARCHAR (260)     NOT NULL,
    [RealFileSize]    BIGINT             NOT NULL,
    [HashSHA256]      NVARCHAR (64)      NOT NULL,
    [IsReadOnly]      BIT                CONSTRAINT [DF_tbl_UserFile_IsReadOnly] DEFAULT ((0)) NOT NULL,
    [CreatedUtc]      DATETIMEOFFSET (7) NOT NULL,
    [LastAccessedUtc] DATETIMEOFFSET (7) NOT NULL,
    [LastUpdatedUtc]  DATETIMEOFFSET (7) NOT NULL,
    [LastVerifiedUtc] DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_tbl_UserFile] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_UserFile_FolderID] FOREIGN KEY ([FolderId]) REFERENCES [dbo].[tbl_UserFolder] ([Id]),
    CONSTRAINT [FK_tbl_UserFile_IdentityID] FOREIGN KEY ([IdentityId]) REFERENCES [dbo].[tbl_User] ([IdentityId]) ON DELETE CASCADE ON UPDATE CASCADE
);














GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_UserFiles]
    ON [dbo].[tbl_UserFile]([Id] ASC);

