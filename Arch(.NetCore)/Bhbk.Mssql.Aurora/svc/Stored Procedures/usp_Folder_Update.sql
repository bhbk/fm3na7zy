CREATE PROCEDURE [svc].[usp_Folder_Update] @Id UNIQUEIDENTIFIER,
	@ParentId UNIQUEIDENTIFIER,
	@VirtualName NVARCHAR(MAX),
	@IsReadOnly BIT,
	@LastAccessedUtc DATETIMEOFFSET(7),
	@LastUpdatedUtc DATETIMEOFFSET(7)
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

		DECLARE @LASTUPDATED DATETIMEOFFSET(7) = GETUTCDATE()

		UPDATE [dbo].[tbl_Folder]
		SET ParentId = @ParentId,
			VirtualName = @VirtualName,
			IsReadOnly = @IsReadOnly,
			LastAccessedUtc = @LastAccessedUtc,
			LastUpdatedUtc = @LastUpdatedUtc
		WHERE Id = @Id
			AND ParentId = @ParentId

		IF @@ROWCOUNT != 1 THROW 51000,
			'ERROR',
			1;
			SELECT *
			FROM [dbo].[tbl_Folder]
			WHERE Id = @Id
				AND ParentId = @ParentId

		COMMIT TRANSACTION;
	END TRY

	BEGIN CATCH
		ROLLBACK TRANSACTION;

		THROW;
	END CATCH
END
