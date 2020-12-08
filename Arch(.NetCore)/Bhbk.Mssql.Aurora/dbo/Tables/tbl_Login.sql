CREATE TABLE [dbo].[tbl_Login] (
    [UserId]               UNIQUEIDENTIFIER   NOT NULL,
    [UserAuthType]         NVARCHAR (16)      NOT NULL,
    [UserName]             NVARCHAR (128)     NOT NULL,
    [FileSystemType]       NVARCHAR (16)      NOT NULL,
    [FileSystemChrootPath] NVARCHAR (64)      NULL,
    [IsPasswordRequired]   BIT                CONSTRAINT [DF_tbl_User_IsPasswordRequired] DEFAULT ((0)) NOT NULL,
    [IsPublicKeyRequired]  BIT                CONSTRAINT [DF_tbl_User_IsPublicKeyRequired] DEFAULT ((0)) NOT NULL,
    [IsFileSystemReadOnly] BIT                CONSTRAINT [DF_tbl_User_IsFileSystemReadOnly] DEFAULT ((0)) NOT NULL,
    [Debugger]             NVARCHAR (16)      NULL,
    [EncryptedPass]        NVARCHAR (1024)    NULL,
    [IsEnabled]            BIT                NOT NULL,
    [IsDeletable]          BIT                NOT NULL,
    [CreatedUtc]           DATETIMEOFFSET (7) NOT NULL,
    [LastUpdatedUtc]       DATETIMEOFFSET (7) NULL,
    CONSTRAINT [PK_tbl_Login] PRIMARY KEY CLUSTERED ([UserId] ASC)
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Login]
    ON [dbo].[tbl_Login]([UserId] ASC);

