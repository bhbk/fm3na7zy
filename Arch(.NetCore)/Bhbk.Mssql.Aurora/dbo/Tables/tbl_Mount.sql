CREATE TABLE [dbo].[tbl_Mount] (
    [UserId]         UNIQUEIDENTIFIER   NOT NULL,
    [AmbassadorId]   UNIQUEIDENTIFIER   NULL,
    [AuthType]       NVARCHAR (16)      NOT NULL,
    [UncPath]        NVARCHAR (256)     NOT NULL,
    [IsEnabled]      BIT                NOT NULL,
    [IsDeletable]    BIT                NOT NULL,
    [CreatedUtc]     DATETIMEOFFSET (7) NOT NULL,
    [LastUpdatedUtc] DATETIMEOFFSET (7) NULL,
    CONSTRAINT [PK_tbl_Mount] PRIMARY KEY CLUSTERED ([UserId] ASC),
    CONSTRAINT [FK_tbl_Mount_AmbassadorID] FOREIGN KEY ([AmbassadorId]) REFERENCES [dbo].[tbl_Ambassador] ([Id]) ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_Mount_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_Login] ([UserId]) ON UPDATE CASCADE
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Mounts]
    ON [dbo].[tbl_Mount]([UserId] ASC);

