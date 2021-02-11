CREATE PROCEDURE [svc].[usp_Network_Update] @Id UNIQUEIDENTIFIER,
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

		UPDATE [dbo].[tbl_Network]
		SET SequenceId = @SequenceId,
			Address = @Address,
			ActionTypeId = @ActionTypeId,
			IsEnabled = @IsEnabled,
			IsDeletable = @IsDeletable,
			Comment = @Comment
		WHERE Id = @Id

		IF @@ROWCOUNT != 1 THROW 51000,
			'ERROR',
			1;
			SELECT *
			FROM [dbo].[tbl_Network]
			WHERE Id = @Id

		COMMIT TRANSACTION;
	END TRY

	BEGIN CATCH
		ROLLBACK TRANSACTION;

		THROW;
	END CATCH
END
