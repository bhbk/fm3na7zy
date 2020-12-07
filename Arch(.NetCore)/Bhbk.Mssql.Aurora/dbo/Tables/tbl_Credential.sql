CREATE TABLE [dbo].[tbl_Credential] (
    [Id]             UNIQUEIDENTIFIER   NOT NULL,
    [Domain]         VARCHAR (128)      NULL,
    [UserName]       VARCHAR (128)      NOT NULL,
    [EncryptedPass]  VARCHAR (128)      NOT NULL,
    [IsEnabled]      BIT                NOT NULL,
    [IsDeletable]    BIT                NOT NULL,
    [CreatedUtc]     DATETIMEOFFSET (7) NOT NULL,
    [LastUpdatedUtc] DATETIMEOFFSET (7) NULL,
    CONSTRAINT [PK_tbl_Credential] PRIMARY KEY CLUSTERED ([Id] ASC)
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Credential]
    ON [dbo].[tbl_Credential]([Id] ASC);

