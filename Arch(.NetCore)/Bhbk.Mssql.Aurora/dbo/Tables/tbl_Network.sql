﻿CREATE TABLE [dbo].[tbl_Network] (
    [Id]             UNIQUEIDENTIFIER   NOT NULL,
    [IdentityId]     UNIQUEIDENTIFIER   NOT NULL,
    [Address]        NVARCHAR (128)     NOT NULL,
    [Action]         NVARCHAR (8)       NOT NULL,
    [IsEnabled]      BIT                NOT NULL,
    [CreatedUtc]     DATETIMEOFFSET (7) NOT NULL,
    [LastUpdatedUtc] DATETIMEOFFSET (7) NULL,
    CONSTRAINT [PK_tbl_Network] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_Network_IdentityID] FOREIGN KEY ([IdentityId]) REFERENCES [dbo].[tbl_User] ([IdentityId]) ON DELETE CASCADE ON UPDATE CASCADE
);












GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Networks]
    ON [dbo].[tbl_Network]([Id] ASC);
