CREATE TABLE [dbo].[tbl_File] (
    [Id]              UNIQUEIDENTIFIER   NOT NULL,
    [FileSystemId]    UNIQUEIDENTIFIER   NOT NULL,
    [CreatorId]       UNIQUEIDENTIFIER   NOT NULL,
    [FolderId]        UNIQUEIDENTIFIER   NOT NULL,
    [VirtualName]     NVARCHAR (260)     NOT NULL,
    [RealPath]        NVARCHAR (MAX)     NOT NULL,
    [RealFileName]    NVARCHAR (260)     NOT NULL,
    [RealFileSize]    BIGINT             NOT NULL,
    [HashTypeId]      INT                NOT NULL,
    [HashValue]       NVARCHAR (64)      NULL,
    [IsReadOnly]      BIT                CONSTRAINT [DF_tbl_File_IsReadOnly] DEFAULT ((0)) NOT NULL,
    [CreatedUtc]      DATETIMEOFFSET (7) NOT NULL,
    [LastAccessedUtc] DATETIMEOFFSET (7) NOT NULL,
    [LastUpdatedUtc]  DATETIMEOFFSET (7) NOT NULL,
    [LastVerifiedUtc] DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_tbl_File] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_File_tbl_FileSystem] FOREIGN KEY ([FileSystemId]) REFERENCES [dbo].[tbl_FileSystem] ([Id]) ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_File_tbl_Folder] FOREIGN KEY ([FolderId]) REFERENCES [dbo].[tbl_Folder] ([Id]),
    CONSTRAINT [FK_tbl_File_tbl_HashAlgorithmType] FOREIGN KEY ([HashTypeId]) REFERENCES [dbo].[tbl_HashAlgorithmType] ([Id]) ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_File_tbl_Login] FOREIGN KEY ([CreatorId]) REFERENCES [dbo].[tbl_Login] ([UserId]) ON UPDATE CASCADE
);










GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_File]
    ON [dbo].[tbl_File]([Id] ASC);

