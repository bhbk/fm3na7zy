CREATE PROCEDURE [svc].[usp_Folder_Insert] @FileSystemId UNIQUEIDENTIFIER,
	@ParentId UNIQUEIDENTIFIER,
	@VirtualName NVARCHAR(MAX),
	@IsReadOnly BIT,
	@CreatorId UNIQUEIDENTIFIER
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

		DECLARE @FOLDERID UNIQUEIDENTIFIER = NEWID()
		DECLARE @CREATEDUTC DATETIMEOFFSET(7) = GETUTCDATE()

		INSERT INTO [dbo].[tbl_Folder] (
			Id,
			FileSystemId,
			ParentId,
			VirtualName,
			IsReadOnly,
			CreatorId,
			CreatedUtc,
			LastAccessedUtc,
			LastUpdatedUtc
			)
		VALUES (
			@FOLDERID,
			@FileSystemId,
			@ParentId,
			@VirtualName,
			@IsReadOnly,
			@CreatorId,
			@CREATEDUTC,
			@CREATEDUTC,
			@CREATEDUTC
			);

		IF @@ROWCOUNT != 1 THROW 51000,
			'ERROR',
			1;
			SELECT *
			FROM [dbo].[tbl_Folder]
			WHERE Id = @FOLDERID

		COMMIT TRANSACTION;
	END TRY

	BEGIN CATCH
		ROLLBACK TRANSACTION;

		THROW;
	END CATCH
END
