CREATE PROCEDURE [svc].[usp_File_Update] @Id UNIQUEIDENTIFIER,
	@FolderId UNIQUEIDENTIFIER,
	@VirtualName NVARCHAR(260),
	@IsReadOnly BIT,
	@RealPath NVARCHAR(MAX),
	@RealFileName NVARCHAR(260),
	@RealFileSize BIGINT,
	@HashTypeId INT,
	@HashValue NVARCHAR(64),
	@LastAccessedUtc DATETIMEOFFSET(7),
	@LastUpdatedUtc DATETIMEOFFSET(7),
	@LastVerifiedUtc DATETIMEOFFSET(7)
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

		UPDATE [dbo].[tbl_File]
		SET FolderId = @FolderId,
			VirtualName = @VirtualName,
			IsReadOnly = @IsReadOnly,
			RealPath = @RealPath,
			RealFileName = @RealFileName,
			RealFileSize = @RealFileSize,
			HashTypeId = @HashTypeId,
			HashValue = @HashValue,
			LastAccessedUtc = @LastAccessedUtc,
			LastUpdatedUtc = @LastUpdatedUtc,
			LastVerifiedUtc = @LastVerifiedUtc
		WHERE Id = @Id
			AND FolderId = @FolderId

		IF @@ROWCOUNT != 1 THROW 51000,
			'ERROR',
			1;
			SELECT *
			FROM [dbo].[tbl_File]
			WHERE Id = @Id
				AND FolderId = @FolderId

		COMMIT TRANSACTION;
	END TRY

	BEGIN CATCH
		ROLLBACK TRANSACTION;

		THROW;
	END CATCH
END
