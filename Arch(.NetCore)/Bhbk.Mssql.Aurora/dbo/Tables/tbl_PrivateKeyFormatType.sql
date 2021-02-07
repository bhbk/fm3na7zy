CREATE TABLE [dbo].[tbl_PrivateKeyFormatType] (
    [Id]          INT            NOT NULL,
    [Name]        NVARCHAR (16)  NOT NULL,
    [Description] NVARCHAR (256) NULL,
    [IsEnabled]   BIT            NOT NULL,
    [IsEditable]  BIT            NOT NULL,
    [IsDeletable] BIT            NOT NULL,
    CONSTRAINT [PK_tbl_PrivateKeyFormatType] PRIMARY KEY CLUSTERED ([Id] ASC)
);








GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_PrivateKeyFormatType_Name]
    ON [dbo].[tbl_PrivateKeyFormatType]([Name] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_PrivateKeyFormatType_ID]
    ON [dbo].[tbl_PrivateKeyFormatType]([Id] ASC);

