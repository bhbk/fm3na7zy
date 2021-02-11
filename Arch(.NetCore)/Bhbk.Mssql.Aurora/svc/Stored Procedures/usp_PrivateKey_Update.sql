CREATE PROCEDURE [svc].[usp_PrivateKey_Update] @Id UNIQUEIDENTIFIER,
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

		UPDATE [dbo].[tbl_PrivateKey]
		SET PublicKeyId = @PublicKeyId,
			KeyValue = @KeyValue,
			KeyAlgorithmId = @KeyAlgorithmId,
			KeyFormatId = @KeyFormatId,
			EncryptedPass = @EncryptedPass,
			IsEnabled = @IsEnabled,
			IsDeletable = @IsDeletable
		WHERE Id = @Id

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
