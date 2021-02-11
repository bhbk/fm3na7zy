CREATE PROCEDURE [svc].[usp_FileSystemLogin_Insert] @FileSystemId UNIQUEIDENTIFIER,
	@UserId UNIQUEIDENTIFIER,
	@SmbAuthTypeId INT,
	@AmbassadorId UNIQUEIDENTIFIER,
	@ChrootPath NVARCHAR(256),
	@IsReadOnly BIT
AS
SET NOCOUNT ON;

DECLARE @CreatedUtc DATETIMEOFFSET(7) = GETUTCDATE()

INSERT INTO [dbo].[tbl_FileSystemLogin] (
	FileSystemId,
	UserId,
	SmbAuthTypeId,
	AmbassadorId,
	ChrootPath,
	CreatedUtc,
	IsReadOnly
	)
VALUES (
	@FileSystemId,
	@UserId,
	@SmbAuthTypeId,
	@AmbassadorId,
	@ChrootPath,
	@CreatedUtc,
	@IsReadOnly
	);

/*  Select all entity values to return
        ----------------------------------------------------
       */
SELECT *
FROM [dbo].[tbl_FileSystemLogin]
WHERE FileSystemId = @FileSystemId
	AND UserId = @UserId
