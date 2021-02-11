CREATE PROCEDURE [svc].[usp_PrivateKey_Insert] @Id UNIQUEIDENTIFIER,
	@UserId UNIQUEIDENTIFIER,
	@PublicKeyId UNIQUEIDENTIFIER,
	@KeyValue NVARCHAR(MAX),
	@KeyAlgorithmId INT,
	@KeyFormatId INT,
	@EncryptedPass NVARCHAR(1024),
	@IsEnabled BIT,
	@IsDeletable BIT
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

		DECLARE @CREATEDUTC DATETIMEOFFSET(7) = GETUTCDATE()

		INSERT INTO [dbo].[tbl_PrivateKey] (
			Id,
			UserId,
			PublicKeyId,
			KeyValue,
			KeyAlgorithmId,
			KeyFormatId,
			EncryptedPass,
			IsEnabled,
			IsDeletable,
			CreatedUtc
			)
		VALUES (
			@Id,
			@UserId,
			@PublicKeyId,
			@KeyValue,
			@KeyAlgorithmId,
			@KeyFormatId,
			@EncryptedPass,
			@IsEnabled,
			@IsDeletable,
			@CREATEDUTC
			);

		IF @@ROWCOUNT != 1 THROW 51000,
			'ERROR',
			1;
			SELECT *
			FROM [dbo].[tbl_PrivateKey]
			WHERE Id = @Id

		COMMIT TRANSACTION;
	END TRY

	BEGIN CATCH
		ROLLBACK TRANSACTION;

		THROW;
	END CATCH
END
