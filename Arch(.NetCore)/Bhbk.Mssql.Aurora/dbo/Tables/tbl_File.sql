CREATE TABLE [dbo].[tbl_File] (
    [Id]              UNIQUEIDENTIFIER   NOT NULL,
    [UserId]          UNIQUEIDENTIFIER   NOT NULL,
    [FolderId]        UNIQUEIDENTIFIER   NOT NULL,
    [VirtualName]     NVARCHAR (260)     NOT NULL,
    [RealPath]        NVARCHAR (MAX)     NOT NULL,
    [RealFileName]    NVARCHAR (260)     NOT NULL,
    [RealFileSize]    BIGINT             NOT NULL,
    [HashSHA256]      NVARCHAR (64)      NOT NULL,
    [IsReadOnly]      BIT                CONSTRAINT [DF_tbl_File_IsReadOnly] DEFAULT ((0)) NOT NULL,
    [CreatedUtc]      DATETIMEOFFSET (7) NOT NULL,
    [LastAccessedUtc] DATETIMEOFFSET (7) NOT NULL,
    [LastUpdatedUtc]  DATETIMEOFFSET (7) NOT NULL,
    [LastVerifiedUtc] DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_tbl_File] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_File_FolderID] FOREIGN KEY ([FolderId]) REFERENCES [dbo].[tbl_Folder] ([Id]),
    CONSTRAINT [FK_tbl_File_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_Login] ([UserId]) ON UPDATE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_File]
    ON [dbo].[tbl_File]([Id] ASC);

