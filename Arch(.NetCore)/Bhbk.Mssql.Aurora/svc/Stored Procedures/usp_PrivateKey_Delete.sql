
CREATE   PROCEDURE [svc].[usp_PrivateKey_Delete]
    @ID uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_PrivateKey] WHERE Id = @ID

        DELETE [dbo].[tbl_PrivateKey]
        WHERE Id = @ID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END