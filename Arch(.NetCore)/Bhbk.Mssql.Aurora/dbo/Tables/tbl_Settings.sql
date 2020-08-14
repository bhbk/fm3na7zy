﻿CREATE TABLE [dbo].[tbl_Settings] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [UserId]      UNIQUEIDENTIFIER NULL,
    [ConfigKey]   VARCHAR (128)    NOT NULL,
    [ConfigValue] VARCHAR (256)    NOT NULL,
    [Created]     DATETIME2 (7)    NOT NULL,
    [Immutable]   BIT              CONSTRAINT [DF_tbl_Settings_Immutable] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_tbl_Settings] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_Settings_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_Users] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);






GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Settings]
    ON [dbo].[tbl_Settings]([Id] ASC);

