CREATE PROCEDURE [svc].[usp_FileSystem_Insert] @FileSystemTypeId INT,
	@Name NVARCHAR(128),
	@Description NVARCHAR(256),
	@UncPath NVARCHAR(256),
	@IsEnabled BIT,
	@IsDeletable BIT
AS
SET NOCOUNT ON;

DECLARE @Id UNIQUEIDENTIFIER = NEWID()
DECLARE @CreatedUtc DATETIMEOFFSET(7) = GETUTCDATE()

INSERT INTO [dbo].[tbl_FileSystem] (
	Id,
	FileSystemTypeId,
	Name,
	Description,
	UncPath,
	CreatedUtc,
	IsEnabled,
	IsDeletable
	)
VALUES (
	@Id,
	@FileSystemTypeId,
	@Name,
	@Description,
	@UncPath,
	@CreatedUtc,
	@IsEnabled,
	@IsDeletable
	);

INSERT INTO [dbo].[tbl_FileSystemUsage] (
	FileSystemId,
	QuotaInBytes,
	QuotaUsedInBytes
	)
VALUES (
	@Id,
	1073741824,
	0
	);

/*  Select all entity values to return
        ----------------------------------------------------
       */
SELECT *
FROM [dbo].[tbl_FileSystem]
WHERE Id = @Id
