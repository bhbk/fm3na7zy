CREATE TABLE [dbo].[tbl_Activity] (
    [Id]             UNIQUEIDENTIFIER   NOT NULL,
    [ActorId]        UNIQUEIDENTIFIER   NULL,
    [IdentityId]     UNIQUEIDENTIFIER   NULL,
    [ActivityType]   NVARCHAR (64)      NOT NULL,
    [TableName]      NVARCHAR (256)     NULL,
    [KeyValues]      NVARCHAR (MAX)     NULL,
    [OriginalValues] NVARCHAR (MAX)     NULL,
    [CurrentValues]  NVARCHAR (MAX)     NULL,
    [IsDeletable]    BIT                NOT NULL,
    [CreatedUtc]     DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_tbl_Activity] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_Activity_UserID] FOREIGN KEY ([IdentityId]) REFERENCES [dbo].[tbl_User] ([IdentityId]) ON DELETE CASCADE ON UPDATE CASCADE
);









