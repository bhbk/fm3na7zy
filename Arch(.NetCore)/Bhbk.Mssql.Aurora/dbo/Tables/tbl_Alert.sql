CREATE TABLE [dbo].[tbl_Alert] (
    [Id]             UNIQUEIDENTIFIER   NOT NULL,
    [UserId]         UNIQUEIDENTIFIER   NOT NULL,
    [OnDelete]       BIT                NOT NULL,
    [OnDownload]     BIT                NOT NULL,
    [OnUpload]       BIT                NOT NULL,
    [ToDisplayName]  NVARCHAR (128)     NOT NULL,
    [ToEmailAddress] NVARCHAR (320)     NULL,
    [ToPhoneNumber]  NVARCHAR (15)      NULL,
    [IsEnabled]      BIT                NOT NULL,
    [Comment]             NVARCHAR (256)     NULL,
    [CreatedUtc]     DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_tbl_Alert] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_Alert_tbl_Login] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_Login] ([UserId]) ON UPDATE CASCADE
);






GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Alert]
    ON [dbo].[tbl_Alert]([Id] ASC);

