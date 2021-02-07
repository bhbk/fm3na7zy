CREATE TABLE [dbo].[tbl_NetworkActionType] (
    [Id]          INT            NOT NULL,
    [Name]        NVARCHAR (8)   NOT NULL,
    [Description] NVARCHAR (256) NULL,
    [IsEnabled]   BIT            NOT NULL,
    [IsEditable]  BIT            NOT NULL,
    [IsDeletable] BIT            NOT NULL,
    CONSTRAINT [PK_tbl_NetworkActionType] PRIMARY KEY CLUSTERED ([Id] ASC)
);








GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_NetworkActionType_Name]
    ON [dbo].[tbl_NetworkActionType]([Name] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_NetworkActionType_ID]
    ON [dbo].[tbl_NetworkActionType]([Id] ASC);

