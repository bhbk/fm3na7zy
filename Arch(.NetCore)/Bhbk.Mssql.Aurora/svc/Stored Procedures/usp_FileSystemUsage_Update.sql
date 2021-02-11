CREATE PROCEDURE [svc].[usp_FileSystemUsage_Update] @FileSystemId UNIQUEIDENTIFIER,
	@QuotaInBytes BIGINT,
	@QuotaUsedInBytes BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
		UPDATE [dbo].[tbl_FileSystemUsage]
		SET QuotaInBytes = @QuotaInBytes,
			QuotaUsedInBytes = @QuotaUsedInBytes
		WHERE FileSystemId = @FileSystemId

		IF @@ROWCOUNT != 1 THROW 51000,
			'ERROR',
			1;
			SELECT *
			FROM [dbo].[tbl_FileSystemUsage]
			WHERE FileSystemId = @FileSystemId
	END TRY

	BEGIN CATCH
		THROW;
	END CATCH
END
