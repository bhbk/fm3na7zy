
CREATE   PROCEDURE [svc].[usp_PublicKey_Delete]
    @ID uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_PublicKey] WHERE Id = @ID

        DELETE [dbo].[tbl_PublicKey]
        WHERE Id = @ID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END