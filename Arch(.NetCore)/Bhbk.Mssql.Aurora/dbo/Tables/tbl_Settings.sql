CREATE TABLE [dbo].[tbl_Settings] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [IdentityId]  UNIQUEIDENTIFIER NULL,
    [ConfigKey]   VARCHAR (128)    NOT NULL,
    [ConfigValue] VARCHAR (256)    NOT NULL,
    [Created]     DATETIME2 (7)    NOT NULL,
    [Immutable]   BIT              CONSTRAINT [DF_tbl_Settings_Immutable] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_tbl_Settings] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_Settings_IdentityID] FOREIGN KEY ([IdentityId]) REFERENCES [dbo].[tbl_Users] ([IdentityId]) ON DELETE CASCADE ON UPDATE CASCADE
);










GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Settings]
    ON [dbo].[tbl_Settings]([Id] ASC);

