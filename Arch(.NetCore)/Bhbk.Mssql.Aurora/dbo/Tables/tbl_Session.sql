CREATE TABLE [dbo].[tbl_Session] (
    [Id]                       UNIQUEIDENTIFIER   NOT NULL,
    [IdentityId]               UNIQUEIDENTIFIER   NULL,
    [CallPath]                 VARCHAR (256)      NOT NULL,
    [Details]                  VARCHAR (MAX)      NULL,
    [LocalEndPoint]            VARCHAR (128)      NOT NULL,
    [LocalSoftwareIdentifier]  VARCHAR (128)      NULL,
    [RemoteEndPoint]           VARCHAR (128)      NOT NULL,
    [RemoteSoftwareIdentifier] VARCHAR (128)      NULL,
    [IsActive]                 BIT                NOT NULL,
    [CreatedUtc]               DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_tbl_Session] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_Session_IdentityID] FOREIGN KEY ([IdentityId]) REFERENCES [dbo].[tbl_UserLogin] ([IdentityId]) ON UPDATE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Session]
    ON [dbo].[tbl_Session]([Id] ASC);

