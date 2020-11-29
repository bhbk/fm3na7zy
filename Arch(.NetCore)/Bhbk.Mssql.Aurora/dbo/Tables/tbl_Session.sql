CREATE TABLE [dbo].[tbl_Session] (
    [Id]                       UNIQUEIDENTIFIER   NOT NULL,
    [IdentityId]               UNIQUEIDENTIFIER   NULL,
    [CallPath]                 VARCHAR (256)      NOT NULL,
    [Details]                  VARCHAR (MAX)      NULL,
    [LocalEndPoint]            VARCHAR (128)      NULL,
    [LocalSoftwareIdentifier]  VARCHAR (128)      NULL,
    [RemoteEndPoint]           VARCHAR (128)      NULL,
    [RemoteSoftwareIdentifier] VARCHAR (128)      NULL,
    [CreatedUtc]               DATETIMEOFFSET (7) NOT NULL,
);
