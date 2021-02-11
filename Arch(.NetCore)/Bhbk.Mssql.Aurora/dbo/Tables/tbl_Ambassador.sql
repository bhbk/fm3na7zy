CREATE TABLE [dbo].[tbl_Ambassador] (
    [Id]                UNIQUEIDENTIFIER   NOT NULL,
    [UserPrincipalName] NVARCHAR (128)     NOT NULL,
    [EncryptedPass]     NVARCHAR (128)     NOT NULL,
    [IsEnabled]         BIT                NOT NULL,
    [IsDeletable]       BIT                NOT NULL,
    [CreatedUtc]        DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_tbl_Ambassador] PRIMARY KEY CLUSTERED ([Id] ASC)
);






GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Ambassador]
    ON [dbo].[tbl_Ambassador]([Id] ASC);

