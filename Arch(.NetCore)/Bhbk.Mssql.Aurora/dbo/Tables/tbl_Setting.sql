CREATE TABLE [dbo].[tbl_Setting] (
    [Id]          UNIQUEIDENTIFIER   NOT NULL,
    [UserId]      UNIQUEIDENTIFIER   NULL,
    [ConfigKey]   NVARCHAR (128)     NOT NULL,
    [ConfigValue] NVARCHAR (256)     NOT NULL,
    [IsDeletable] BIT                NOT NULL,
    [CreatedUtc]  DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_tbl_Setting] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_Setting_tbl_Login] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_Login] ([UserId]) ON UPDATE CASCADE
);










GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Setting]
    ON [dbo].[tbl_Setting]([Id] ASC);

