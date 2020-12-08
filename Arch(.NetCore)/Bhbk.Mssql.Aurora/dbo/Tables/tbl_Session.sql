CREATE TABLE [dbo].[tbl_Session] (
    [Id]                       UNIQUEIDENTIFIER   NOT NULL,
    [UserId]                   UNIQUEIDENTIFIER   NULL,
    [CallPath]                 NVARCHAR (256)     NOT NULL,
    [Details]                  NVARCHAR (MAX)     NULL,
    [LocalEndPoint]            NVARCHAR (128)     NOT NULL,
    [LocalSoftwareIdentifier]  NVARCHAR (128)     NULL,
    [RemoteEndPoint]           NVARCHAR (128)     NOT NULL,
    [RemoteSoftwareIdentifier] NVARCHAR (128)     NULL,
    [IsActive]                 BIT                NOT NULL,
    [CreatedUtc]               DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_tbl_Session] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_Session_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_Login] ([UserId]) ON UPDATE CASCADE
);






GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Session]
    ON [dbo].[tbl_Session]([Id] ASC);

