CREATE PROCEDURE [svc].[usp_Alert_Insert] @UserId UNIQUEIDENTIFIER,
	@OnDelete BIT,
	@OnDownload BIT,
	@OnUpload BIT,
	@ToDisplayName NVARCHAR(256),
	@ToEmailAddress NVARCHAR(320),
	@ToPhoneNumber NVARCHAR(15),
	@IsEnabled BIT,
	@Comment NVARCHAR(256)
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

		DECLARE @ALERTID UNIQUEIDENTIFIER = NEWID()
		DECLARE @CREATEDUTC DATETIMEOFFSET(7) = GETUTCDATE()

		INSERT INTO [dbo].[tbl_Alert] (
			Id,
			UserId,
			OnDelete,
			OnDownload,
			OnUpload,
			ToDisplayName,
			ToEmailAddress,
			ToPhoneNumber,
			IsEnabled,
			Comment,
			CreatedUtc
			)
		VALUES (
			@ALERTID,
			@UserId,
			@OnDelete,
			@OnDownload,
			@OnUpload,
			@ToDisplayName,
			@ToEmailAddress,
			@ToPhoneNumber,
			@IsEnabled,
			@Comment,
			@CREATEDUTC
			);

		IF @@ROWCOUNT != 1 THROW 51000,
			'ERROR',
			1;
			SELECT *
			FROM [dbo].[tbl_Alert]
			WHERE Id = @ALERTID

		COMMIT TRANSACTION;
	END TRY

	BEGIN CATCH
		ROLLBACK TRANSACTION;

		THROW;
	END CATCH
END
