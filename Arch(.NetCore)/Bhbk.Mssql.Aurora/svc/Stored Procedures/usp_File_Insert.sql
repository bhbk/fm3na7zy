CREATE PROCEDURE [svc].[usp_File_Insert] @FileSystemId UNIQUEIDENTIFIER,
	@FolderId UNIQUEIDENTIFIER,
	@VirtualName NVARCHAR(260),
	@IsReadOnly BIT,
	@RealPath NVARCHAR(MAX),
	@RealFileName NVARCHAR(260),
	@RealFileSize BIGINT,
	@HashTypeId INT,
	@HashValue NVARCHAR(64),
	@CreatorId UNIQUEIDENTIFIER
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

		DECLARE @FILEID UNIQUEIDENTIFIER = NEWID()
		DECLARE @CREATEDUTC DATETIMEOFFSET(7) = GETUTCDATE()

		INSERT INTO [dbo].[tbl_File] (
			Id,
			FileSystemId,
			FolderId,
			VirtualName,
			IsReadOnly,
			RealPath,
			RealFileName,
			RealFileSize,
			HashTypeId,
			HashValue,
			CreatorId,
			CreatedUtc,
			LastAccessedUtc,
			LastUpdatedUtc,
			LastVerifiedUtc
			)
		VALUES (
			@FILEID,
			@FileSystemId,
			@FolderId,
			@VirtualName,
			@IsReadOnly,
			@RealPath,
			@RealFileName,
			@RealFileSize,
			@HashTypeId,
			@HashValue,
			@CreatorId,
			@CREATEDUTC,
			@CREATEDUTC,
			@CREATEDUTC,
			@CREATEDUTC
			);

		IF @@ROWCOUNT != 1 THROW 51000,
			'ERROR',
			1;
			SELECT *
			FROM [dbo].[tbl_File]
			WHERE Id = @FILEID

		COMMIT TRANSACTION;
	END TRY

	BEGIN CATCH
		ROLLBACK TRANSACTION;

		THROW;
	END CATCH
END
