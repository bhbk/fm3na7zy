CREATE TABLE [dbo].[tbl_KeyAlgorithmType] (
    [Id]          INT            NOT NULL,
    [Name]        NVARCHAR (16)  NOT NULL,
    [Description] NVARCHAR (256) NULL,
    [IsEnabled]   BIT            NOT NULL,
    [IsEditable]  BIT            NOT NULL,
    [IsDeletable] BIT            NOT NULL,
    CONSTRAINT [PK_tbl_KeyAlgorithmType] PRIMARY KEY CLUSTERED ([Id] ASC)
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_KeyAlgorithmType_Name]
    ON [dbo].[tbl_KeyAlgorithmType]([Name] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_KeyAlgorithmType_ID]
    ON [dbo].[tbl_KeyAlgorithmType]([Id] ASC);

