CREATE TABLE [dbo].[tbl_UserFiles] (
    [Id]              UNIQUEIDENTIFIER NOT NULL,
    [UserId]          UNIQUEIDENTIFIER NOT NULL,
    [VirtualParentId] UNIQUEIDENTIFIER NULL,
    [VirtualFileName] NVARCHAR (MAX)   NOT NULL,
    [RealFolder]      NVARCHAR (MAX)   NOT NULL,
    [RealFileName]    NVARCHAR (MAX)   NOT NULL,
    [FileSize]        INT              NOT NULL,
    [FileHashSHA256]  NVARCHAR (64)    NOT NULL,
    [Created]         DATETIME2 (7)    NOT NULL,
    [LastAccessed]    DATETIME2 (7)    NULL,
    [LastUpdated]     DATETIME2 (7)    NULL,
    CONSTRAINT [PK_tbl_UserFiles] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_UserFiles_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_Users] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);



