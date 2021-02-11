CREATE TABLE [dbo].[tbl_FileSystem] (
    [Id]               UNIQUEIDENTIFIER   NOT NULL,
    [FileSystemTypeId] INT                NOT NULL,
    [Name]             NVARCHAR (128)     NOT NULL,
    [Description]      NVARCHAR (256)     NULL,
    [UncPath]          NVARCHAR (256)     NULL,
    [CreatedUtc]       DATETIMEOFFSET (7) NOT NULL,
    [IsEnabled]        BIT                NOT NULL,
    [IsDeletable]      BIT                NOT NULL,
    CONSTRAINT [PK_tbl_FileSystem_ID] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_FileSystem_tbl_FileSystemType] FOREIGN KEY ([FileSystemTypeId]) REFERENCES [dbo].[tbl_FileSystemType] ([Id]) ON UPDATE CASCADE
);



GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_FileSystem_Name]
    ON [dbo].[tbl_FileSystem]([Name] ASC);

