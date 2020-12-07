CREATE TABLE [dbo].[tbl_Setting] (
    [Id]             UNIQUEIDENTIFIER   NOT NULL,
    [IdentityId]     UNIQUEIDENTIFIER   NULL,
    [ConfigKey]      VARCHAR (128)      NOT NULL,
    [ConfigValue]    VARCHAR (256)      NOT NULL,
    [IsDeletable]    BIT                NOT NULL,
    [CreatedUtc]     DATETIMEOFFSET (7) NOT NULL,
    [LastUpdatedUtc] DATETIMEOFFSET (7) NULL,
    CONSTRAINT [PK_tbl_Setting] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_Setting_IdentityID] FOREIGN KEY ([IdentityId]) REFERENCES [dbo].[tbl_UserLogin] ([IdentityId]) ON UPDATE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Setting]
    ON [dbo].[tbl_Setting]([Id] ASC);

