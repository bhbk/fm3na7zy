CREATE TABLE [dbo].[tbl_FileSystemLogin] (
    [FileSystemId]  UNIQUEIDENTIFIER   NOT NULL,
    [UserId]        UNIQUEIDENTIFIER   NOT NULL,
    [SmbAuthTypeId] INT                NULL,
    [AmbassadorId]  UNIQUEIDENTIFIER   NULL,
    [ChrootPath]    NVARCHAR (128)     NULL,
    [CreatedUtc]    DATETIMEOFFSET (7) NOT NULL,
    [IsReadOnly]    BIT                CONSTRAINT [DF_tbl_FileSystemLogin_IsReadOnly] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_tbl_FileSystemLogin] PRIMARY KEY CLUSTERED ([UserId] ASC, [FileSystemId] ASC),
    CONSTRAINT [FK_tbl_FileSystemLogin_tbl_Ambassador] FOREIGN KEY ([AmbassadorId]) REFERENCES [dbo].[tbl_Ambassador] ([Id]) ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_FileSystemLogin_tbl_FileSystem] FOREIGN KEY ([FileSystemId]) REFERENCES [dbo].[tbl_FileSystem] ([Id]) ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_FileSystemLogin_tbl_Login] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_Login] ([UserId]) ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_FileSystemLogin_tbl_SmbAuthType] FOREIGN KEY ([SmbAuthTypeId]) REFERENCES [dbo].[tbl_SmbAuthType] ([Id]) ON UPDATE CASCADE
);






GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_FileSystemLogin]
    ON [dbo].[tbl_FileSystemLogin]([UserId] ASC, [FileSystemId] ASC);

