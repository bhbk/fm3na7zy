CREATE TABLE [dbo].[tbl_Folder] (
    [Id]              UNIQUEIDENTIFIER   NOT NULL,
    [FileSystemId]    UNIQUEIDENTIFIER   NOT NULL,
    [CreatorId]       UNIQUEIDENTIFIER   NOT NULL,
    [ParentId]        UNIQUEIDENTIFIER   NULL,
    [VirtualName]     NVARCHAR (MAX)     NOT NULL,
    [IsReadOnly]      BIT                CONSTRAINT [DF_tbl_Folder_IsReadOnly] DEFAULT ((0)) NOT NULL,
    [CreatedUtc]      DATETIMEOFFSET (7) NOT NULL,
    [LastAccessedUtc] DATETIMEOFFSET (7) NOT NULL,
    [LastUpdatedUtc]  DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_tbl_Folder] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_Folder_tbl_FileSystem] FOREIGN KEY ([FileSystemId]) REFERENCES [dbo].[tbl_FileSystem] ([Id]) ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_Folder_tbl_Folder_Parent] FOREIGN KEY ([ParentId]) REFERENCES [dbo].[tbl_Folder] ([Id]),
    CONSTRAINT [FK_tbl_Folder_tbl_Login] FOREIGN KEY ([CreatorId]) REFERENCES [dbo].[tbl_Login] ([UserId]) ON UPDATE CASCADE
);






GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Folder]
    ON [dbo].[tbl_Folder]([Id] ASC);

