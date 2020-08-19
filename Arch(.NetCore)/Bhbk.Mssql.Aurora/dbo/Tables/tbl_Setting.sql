CREATE TABLE [dbo].[tbl_Setting] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [IdentityId]  UNIQUEIDENTIFIER NULL,
    [ConfigKey]   VARCHAR (128)    NOT NULL,
    [ConfigValue] VARCHAR (256)    NOT NULL,
    [Deletable]   BIT              CONSTRAINT [DF_tbl_Settings_Deletable] DEFAULT ((0)) NOT NULL,
    [Created]     DATETIME2 (7)    NOT NULL,
    [LastUpdated] DATETIME2 (7)    NULL,
    CONSTRAINT [PK_tbl_Settings] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_Setting_IdentityID] FOREIGN KEY ([IdentityId]) REFERENCES [dbo].[tbl_User] ([IdentityId]) ON DELETE CASCADE ON UPDATE CASCADE
);






GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Settings]
    ON [dbo].[tbl_Setting]([Id] ASC);

