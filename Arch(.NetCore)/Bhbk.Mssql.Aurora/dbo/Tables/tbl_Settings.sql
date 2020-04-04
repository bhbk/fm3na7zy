﻿CREATE TABLE [dbo].[tbl_Settings] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [ConfigKey]   VARCHAR (128)    NOT NULL,
    [ConfigValue] VARCHAR (256)    NOT NULL,
    [Created]     DATETIME2 (7)    NOT NULL,
    [Immutable]   BIT              CONSTRAINT [DF_tbl_Settings_Immutable] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Settings] PRIMARY KEY CLUSTERED ([Id] ASC)
);



