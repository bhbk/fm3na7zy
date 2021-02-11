CREATE PROCEDURE [svc].[usp_LoginUsage_Update] @UserId UNIQUEIDENTIFIER,
	@SessionMax SMALLINT,
	@SessionsInUse SMALLINT
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
		UPDATE [dbo].[tbl_LoginUsage]
		SET SessionMax = @SessionMax,
			SessionsInUse = @SessionsInUse
		WHERE UserId = @UserId

		IF @@ROWCOUNT != 1 THROW 51000,
			'ERROR',
			1;
			SELECT *
			FROM [dbo].[tbl_LoginUsage]
			WHERE UserId = @UserId
	END TRY

	BEGIN CATCH
		THROW;
	END CATCH
END
