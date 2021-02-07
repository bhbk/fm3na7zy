CREATE TABLE [dbo].[tbl_PublicKeySignatureType] (
    [Id]          INT            NOT NULL,
    [Name]        NVARCHAR (16)  NOT NULL,
    [Description] NVARCHAR (256) NULL,
    [IsEnabled]   BIT            NOT NULL,
    [IsEditable]  BIT            NOT NULL,
    [IsDeletable] BIT            NOT NULL,
    CONSTRAINT [PK_tbl_PublicKeySignatureType] PRIMARY KEY CLUSTERED ([Id] ASC)
);








GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_PublicKeySignatureType_Name]
    ON [dbo].[tbl_PublicKeySignatureType]([Name] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_PublicKeySignatureType_ID]
    ON [dbo].[tbl_PublicKeySignatureType]([Id] ASC);

