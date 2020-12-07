CREATE TABLE [dbo].[tbl_UserAlert] (
    [Id]             UNIQUEIDENTIFIER   NOT NULL,
    [IdentityId]     UNIQUEIDENTIFIER   NOT NULL,
    [OnDelete]       BIT                NOT NULL,
    [OnDownload]     BIT                NOT NULL,
    [OnUpload]       BIT                NOT NULL,
    [ToDisplayName]  VARCHAR (128)      NOT NULL,
    [ToEmailAddress] VARCHAR (320)      NULL,
    [ToPhoneNumber]  VARCHAR (15)       NULL,
    [IsEnabled]      BIT                NOT NULL,
    [CreatedUtc]     DATETIMEOFFSET (7) NOT NULL,
    [LastUpdatedUtc] DATETIMEOFFSET (7) NULL,
    CONSTRAINT [PK_tbl_UserAlert] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_UserAlert_IdentityID] FOREIGN KEY ([IdentityId]) REFERENCES [dbo].[tbl_UserLogin] ([IdentityId]) ON UPDATE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_UserAlert]
    ON [dbo].[tbl_UserAlert]([Id] ASC);

