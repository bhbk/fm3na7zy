CREATE TABLE [dbo].[tbl_Login] (
    [UserId]               UNIQUEIDENTIFIER   NOT NULL,
    [UserName]             NVARCHAR (128)     NOT NULL,
    [AuthTypeId]           INT                NOT NULL,
    [FileSystemTypeId]     INT                NOT NULL,
    [FileSystemChrootPath] NVARCHAR (64)      NULL,
    [IsPasswordRequired]   BIT                CONSTRAINT [DF_tbl_User_IsPasswordRequired] DEFAULT ((0)) NOT NULL,
    [IsPublicKeyRequired]  BIT                CONSTRAINT [DF_tbl_User_IsPublicKeyRequired] DEFAULT ((0)) NOT NULL,
    [IsFileSystemReadOnly] BIT                CONSTRAINT [DF_tbl_User_IsFileSystemReadOnly] DEFAULT ((0)) NOT NULL,
    [DebugTypeId]          INT                NOT NULL,
    [EncryptedPass]        NVARCHAR (1024)    NULL,
    [IsEnabled]            BIT                NOT NULL,
    [IsDeletable]          BIT                NOT NULL,
    [CreatedUtc]           DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_tbl_Login] PRIMARY KEY CLUSTERED ([UserId] ASC),
    CONSTRAINT [FK_tbl_Login_tbl_FileSystemType] FOREIGN KEY ([FileSystemTypeId]) REFERENCES [dbo].[tbl_FileSystemType] ([Id]) ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_Login_tbl_LoginAuthType] FOREIGN KEY ([AuthTypeId]) REFERENCES [dbo].[tbl_LoginAuthType] ([Id]) ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_Login_tbl_LoginDebugType] FOREIGN KEY ([DebugTypeId]) REFERENCES [dbo].[tbl_LoginDebugType] ([Id]) ON UPDATE CASCADE
);










GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Login]
    ON [dbo].[tbl_Login]([UserId] ASC);

