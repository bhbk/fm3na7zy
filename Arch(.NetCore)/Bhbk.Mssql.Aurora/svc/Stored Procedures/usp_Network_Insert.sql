CREATE PROCEDURE [svc].[usp_Network_Insert] @UserId UNIQUEIDENTIFIER,
	@SequenceId INT,
	@Address NVARCHAR(128),
	@ActionTypeId INT,
	@IsEnabled BIT,
	@IsDeletable BIT,
	@Comment NVARCHAR(256)
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

		DECLARE @NETWORKID UNIQUEIDENTIFIER = NEWID()
		DECLARE @CREATEDUTC DATETIMEOFFSET(7) = GETUTCDATE()

		INSERT INTO [dbo].[tbl_Network] (
			Id,
			UserId,
			SequenceId,
			Address,
			ActionTypeId,
			IsEnabled,
			IsDeletable,
			Comment,
			CreatedUtc
			)
		VALUES (
			@NETWORKID,
			@UserId,
			@SequenceId,
			@Address,
			@ActionTypeId,
			@IsEnabled,
			@IsDeletable,
			@Comment,
			@CREATEDUTC
			);

		IF @@ROWCOUNT != 1 THROW 51000,
			'ERROR',
			1;
			SELECT *
			FROM [dbo].[tbl_Network]
			WHERE Id = @NETWORKID

		COMMIT TRANSACTION;
	END TRY

	BEGIN CATCH
		ROLLBACK TRANSACTION;

		THROW;
	END CATCH
END
