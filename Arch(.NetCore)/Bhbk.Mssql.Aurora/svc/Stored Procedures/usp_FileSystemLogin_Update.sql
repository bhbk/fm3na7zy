CREATE PROCEDURE [svc].[usp_FileSystemLogin_Update] @FileSystemId UNIQUEIDENTIFIER,
	@UserId UNIQUEIDENTIFIER,
	@SmbAuthTypeId INT,
	@AmbassadorId UNIQUEIDENTIFIER,
	@ChrootPath NVARCHAR(256),
	@IsReadOnly BIT
AS
SET NOCOUNT ON;

UPDATE [dbo].[tbl_FileSystemLogin]
SET SmbAuthTypeId = @SmbAuthTypeId,
	AmbassadorId = @AmbassadorId,
	ChrootPath = @ChrootPath,
	IsReadOnly = @IsReadOnly
WHERE FileSystemId = @FileSystemId
	AND UserId = @UserId;

/*  Select all entity values to return
        ----------------------------------------------------
       */
SELECT *
FROM [dbo].[tbl_FileSystemLogin]
WHERE FileSystemId = @FileSystemId
	AND UserId = @UserId
