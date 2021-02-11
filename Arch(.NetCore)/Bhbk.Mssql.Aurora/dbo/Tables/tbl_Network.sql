CREATE TABLE [dbo].[tbl_Network] (
    [Id]           UNIQUEIDENTIFIER   NOT NULL,
    [UserId]       UNIQUEIDENTIFIER   NULL,
    [SequenceId]   INT                NOT NULL,
    [Address]      NVARCHAR (128)     NOT NULL,
    [ActionTypeId] INT                NOT NULL,
    [IsEnabled]    BIT                NOT NULL,
    [IsDeletable]  BIT                NOT NULL,
    [Comment]             NVARCHAR (256)     NULL,
    [CreatedUtc]   DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_tbl_Network] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_Network_tbl_Login] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_Login] ([UserId]) ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_Network_tbl_NetworkActionType] FOREIGN KEY ([ActionTypeId]) REFERENCES [dbo].[tbl_NetworkActionType] ([Id]) ON UPDATE CASCADE
);












GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Network]
    ON [dbo].[tbl_Network]([Id] ASC);

