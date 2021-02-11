CREATE PROCEDURE [svc].[usp_Ambassador_Insert] @UserPrincipalName NVARCHAR(128),
	@EncryptedPass NVARCHAR(128),
	@IsEnabled BIT,
	@IsDeletable BIT
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

		DECLARE @CREDENTIALID UNIQUEIDENTIFIER = NEWID()
		DECLARE @CREATEDUTC DATETIMEOFFSET(7) = GETUTCDATE()

		INSERT INTO [dbo].[tbl_Ambassador] (
			Id,
			UserPrincipalName,
			EncryptedPass,
			IsEnabled,
			IsDeletable,
			CreatedUtc
			)
		VALUES (
			@CREDENTIALID,
			@UserPrincipalName,
			@EncryptedPass,
			@IsEnabled,
			@IsDeletable,
			@CREATEDUTC
			);

		IF @@ROWCOUNT != 1 THROW 51000,
			'ERROR',
			1;
			SELECT *
			FROM [dbo].[tbl_Ambassador]
			WHERE Id = @CREDENTIALID

		COMMIT TRANSACTION;
	END TRY

	BEGIN CATCH
		ROLLBACK TRANSACTION;

		THROW;
	END CATCH
END
