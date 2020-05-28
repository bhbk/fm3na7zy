CREATE TABLE [dbo].[tbl_SysPrivateKeys] (
    [Id]             UNIQUEIDENTIFIER NOT NULL,
    [KeyValueBase64] NVARCHAR (MAX)   NOT NULL,
    [KeyValueAlgo]   NVARCHAR (16)    NOT NULL,
    [KeyValuePass]   NVARCHAR (1024)  NOT NULL,
    [KeyValueFormat] NVARCHAR (16)    NOT NULL,
    [Enabled]        BIT              CONSTRAINT [DF_tbl_PrivateKeys_Enabled] DEFAULT ((0)) NOT NULL,
    [Created]        DATETIME2 (7)    NOT NULL,
    [Immutable]      BIT              CONSTRAINT [DF_tbl_PrivateKeys_Immutable] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_tbl_PrivateKeys] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_PrivateKeys]
    ON [dbo].[tbl_SysPrivateKeys]([Id] ASC);

