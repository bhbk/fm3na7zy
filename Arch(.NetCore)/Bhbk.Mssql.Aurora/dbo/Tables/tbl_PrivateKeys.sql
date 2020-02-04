CREATE TABLE [dbo].[tbl_PrivateKeys] (
    [Id]             UNIQUEIDENTIFIER NOT NULL,
    [KeyValueBase64] NVARCHAR (2048)  NOT NULL,
    [KeyValueAlgo]   NVARCHAR (16)    NOT NULL,
    [KeyValuePass]   NVARCHAR (1024)  NOT NULL,
    [KeyValueFormat] NVARCHAR (16)    NOT NULL,
    [Enabled]        BIT              NOT NULL,
    [Created]        DATETIME2 (7)    NOT NULL,
    [Immutable]      BIT              NOT NULL,
    CONSTRAINT [PK_tbl_PrivateKeys] PRIMARY KEY CLUSTERED ([Id] ASC)
);








GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_PrivateKeys]
    ON [dbo].[tbl_PrivateKeys]([Id] ASC);

