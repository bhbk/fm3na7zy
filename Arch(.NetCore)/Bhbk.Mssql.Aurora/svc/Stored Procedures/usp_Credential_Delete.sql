
CREATE PROCEDURE [svc].[usp_Credential_Delete]
    @ID uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_Credential] WHERE Id = @ID

        DELETE [dbo].[tbl_Credential]
        WHERE Id = @ID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END