CREATE TABLE [dbo].[tbl_PublicKeyFormatType] (
    [Id]          INT            NOT NULL,
    [Name]        NVARCHAR (16)  NOT NULL,
    [Description] NVARCHAR (256) NULL,
    [IsEnabled]   BIT            NOT NULL,
    [IsEditable]  BIT            NOT NULL,
    [IsDeletable] BIT            NOT NULL,
    CONSTRAINT [PK_tbl_PublicKeyFormatType] PRIMARY KEY CLUSTERED ([Id] ASC)
);








GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_PublicKeyFormatType_Name]
    ON [dbo].[tbl_PublicKeyFormatType]([Name] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_PublicKeyFormatType_ID]
    ON [dbo].[tbl_PublicKeyFormatType]([Id] ASC);

