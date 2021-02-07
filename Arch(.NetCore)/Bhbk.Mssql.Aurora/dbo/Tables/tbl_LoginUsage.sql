CREATE TABLE [dbo].[tbl_LoginUsage] (
    [UserId]           UNIQUEIDENTIFIER NOT NULL,
    [QuotaInBytes]     BIGINT           NOT NULL,
    [QuotaUsedInBytes] BIGINT           NOT NULL,
    [SessionMax]       SMALLINT         NOT NULL,
    [SessionsInUse]    SMALLINT         NOT NULL,
    CONSTRAINT [PK_tbl_Usage] PRIMARY KEY CLUSTERED ([UserId] ASC),
    CONSTRAINT [FK_tbl_Usage_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_Login] ([UserId]) ON UPDATE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Usage]
    ON [dbo].[tbl_LoginUsage]([UserId] ASC);

