CREATE TABLE [dbo].[tbl_UserFolder] (
    [Id]              UNIQUEIDENTIFIER   NOT NULL,
    [IdentityId]      UNIQUEIDENTIFIER   NOT NULL,
    [ParentId]        UNIQUEIDENTIFIER   NULL,
    [VirtualName]     VARCHAR (MAX)      NOT NULL,
    [IsReadOnly]      BIT                CONSTRAINT [DF_tbl_UserFolders_IsReadOnly] DEFAULT ((0)) NOT NULL,
    [CreatedUtc]      DATETIMEOFFSET (7) NOT NULL,
    [LastAccessedUtc] DATETIMEOFFSET (7) NOT NULL,
    [LastUpdatedUtc]  DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_tbl_UserFolder] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_UserFolder_IdentityID] FOREIGN KEY ([IdentityId]) REFERENCES [dbo].[tbl_User] ([IdentityId]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_UserFolder_ParentID] FOREIGN KEY ([ParentId]) REFERENCES [dbo].[tbl_UserFolder] ([Id])
);










GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_UserFolders]
    ON [dbo].[tbl_UserFolder]([Id] ASC);

