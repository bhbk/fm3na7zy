CREATE PROCEDURE [svc].[usp_PublicKey_Delete] @Id UNIQUEIDENTIFIER
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
		SELECT *
		FROM [dbo].[tbl_PublicKey]
		WHERE Id = @Id

		DELETE [dbo].[tbl_PublicKey]
		WHERE Id = @Id
	END TRY

	BEGIN CATCH
		THROW;
	END CATCH
END
