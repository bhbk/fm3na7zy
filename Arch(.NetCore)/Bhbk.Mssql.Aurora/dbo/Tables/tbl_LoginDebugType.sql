CREATE TABLE [dbo].[tbl_LoginDebugType] (
    [Id]          INT            NOT NULL,
    [Name]        NVARCHAR (16)  NOT NULL,
    [Description] NVARCHAR (256) NULL,
    [IsEnabled]   BIT            NOT NULL,
    [IsEditable]  BIT            NOT NULL,
    [IsDeletable] BIT            NOT NULL,
    CONSTRAINT [PK_tbl_LoginDebugType] PRIMARY KEY CLUSTERED ([Id] ASC)
);








GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_LoginDebugType_Name]
    ON [dbo].[tbl_LoginDebugType]([Name] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_LoginDebugType_ID]
    ON [dbo].[tbl_LoginDebugType]([Id] ASC);

