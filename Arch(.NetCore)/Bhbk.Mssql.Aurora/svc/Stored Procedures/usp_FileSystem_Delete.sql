
CREATE PROCEDURE [svc].[usp_FileSystem_Delete]
	@Id uniqueidentifier

AS

SET NOCOUNT ON;

DECLARE @Error_Message nvarchar(max);

BEGIN TRY

IF EXISTS (SELECT 1
		   FROM [dbo].[tbl_FileSystem]
		   WHERE Id = @Id
			   AND IsDeletable = 0)
	BEGIN
		SET @Error_Message = FORMATMESSAGE(
			'DELETE not allowed, Id (%s) is not deletable.'
			,CONVERT(NVARCHAR, COALESCE(@Id, '')));
		THROW 50000, @Error_Message, 1;
	END;

IF EXISTS (SELECT 1
		   FROM [dbo].[tbl_File]
		   WHERE Id = @Id)
	BEGIN
		SET @Error_Message = FORMATMESSAGE(
			'DELETE not allowed, Id (%s) has files.'
			,CONVERT(NVARCHAR, COALESCE(@Id, '')));
		THROW 50000, @Error_Message, 1;
	END;

	BEGIN TRANSACTION;

	-- Delete File (not needed as delete will not progress if files exist for organization)

		--DELETE [dbo].[tbl_File] 
		--WHERE UserId = @UserId;

	-- Delete Folder (not needed as delete will not progress if folders exist for organization)

		--DELETE [dbo].[tbl_Folder] 
		--WHERE UserId = @UserId;

	-- Delete FileSystemUsage

		DELETE [dbo].[tbl_FileSystemUsage] 
		WHERE FileSystemId = @Id;

	-- Delete FileSystem

		DELETE [dbo].[tbl_FileSystem] 
		WHERE Id = @Id;

	COMMIT TRANSACTION;

END TRY

BEGIN CATCH

	ROLLBACK TRANSACTION;
	THROW;

END CATCH
