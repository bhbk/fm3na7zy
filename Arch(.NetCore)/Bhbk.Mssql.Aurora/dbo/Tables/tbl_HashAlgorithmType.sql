CREATE TABLE [dbo].[tbl_HashAlgorithmType] (
    [Id]          INT            NOT NULL,
    [Name]        NVARCHAR (16)  NOT NULL,
    [Description] NVARCHAR (256) NULL,
    [IsEnabled]   BIT            NOT NULL,
    [IsEditable]  BIT            NOT NULL,
    [IsDeletable] BIT            NOT NULL,
    CONSTRAINT [PK_tbl_HashAlgorithmType] PRIMARY KEY CLUSTERED ([Id] ASC)
);

