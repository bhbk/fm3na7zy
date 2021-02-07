CREATE TABLE [dbo].[tbl_FileSystemUsage] (
    [FileSystemId]     UNIQUEIDENTIFIER NOT NULL,
    [QuotaInBytes]     BIGINT           NOT NULL,
    [QuotaUsedInBytes] BIGINT           NOT NULL,
    CONSTRAINT [PK_tbl_FileSystemUsage] PRIMARY KEY CLUSTERED ([FileSystemId] ASC),
    CONSTRAINT [FK_tbl_FileSystemUsage_FileSystemID] FOREIGN KEY ([FileSystemId]) REFERENCES [dbo].[tbl_FileSystem] ([Id]) ON UPDATE CASCADE
);

