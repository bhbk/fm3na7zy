CREATE TABLE [dbo].[tbl_SmbAuthType] (
    [Id]          INT            NOT NULL,
    [Name]        NVARCHAR (32)  NOT NULL,
    [Description] NVARCHAR (256) NULL,
    [IsEnabled]   BIT            NOT NULL,
    [IsEditable]  BIT            NOT NULL,
    [IsDeletable] BIT            NOT NULL,
    CONSTRAINT [PK_tbl_SmbAuthType] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_SmbAuthType_Name]
    ON [dbo].[tbl_SmbAuthType]([Name] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_SmbAuthType_ID]
    ON [dbo].[tbl_SmbAuthType]([Id] ASC);

