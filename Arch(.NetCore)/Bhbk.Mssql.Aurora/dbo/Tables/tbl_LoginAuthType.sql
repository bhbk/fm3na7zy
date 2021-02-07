CREATE TABLE [dbo].[tbl_LoginAuthType] (
    [Id]          INT            NOT NULL,
    [Name]        NVARCHAR (16)  NOT NULL,
    [Description] NVARCHAR (256) NULL,
    [IsEnabled]   BIT            NOT NULL,
    [IsEditable]  BIT            NOT NULL,
    [IsDeletable] BIT            NOT NULL,
    CONSTRAINT [PK_tbl_LoginAuthType] PRIMARY KEY CLUSTERED ([Id] ASC)
);








GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_LoginAuthType_Name]
    ON [dbo].[tbl_LoginAuthType]([Name] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_LoginAuthType_ID]
    ON [dbo].[tbl_LoginAuthType]([Id] ASC);

