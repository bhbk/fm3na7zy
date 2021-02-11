CREATE PROCEDURE [svc].[usp_Login_Update] @UserId UNIQUEIDENTIFIER,
	@UserName NVARCHAR(128),
	@AuthTypeId INT,
	@IsPasswordRequired BIT,
	@IsPublicKeyRequired BIT,
	@EncryptedPass NVARCHAR(1024),
	@Comment NVARCHAR(256),
	@DebugTypeId INT,
	@IsEnabled BIT,
	@IsDeletable BIT
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

		UPDATE [dbo].[tbl_Login]
		SET UserName = @UserName,
			AuthTypeId = @AuthTypeId,
			IsPasswordRequired = @IsPasswordRequired,
			IsPublicKeyRequired = @IsPublicKeyRequired,
			EncryptedPass = @EncryptedPass,
			Comment = @Comment,
			DebugTypeId = @DebugTypeId,
			IsEnabled = @IsEnabled,
			IsDeletable = @IsDeletable
		WHERE UserId = @UserId

		IF @@ROWCOUNT != 1 THROW 51000,
			'ERROR',
			1;
			SELECT *
			FROM [dbo].[tbl_Login]
			WHERE UserId = @UserId

		COMMIT TRANSACTION;
	END TRY

	BEGIN CATCH
		ROLLBACK TRANSACTION;

		THROW;
	END CATCH
END
