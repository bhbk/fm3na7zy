



CREATE   PROCEDURE [svc].[usp_UserFolder_Delete]
    @ID uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_UserFolder] WHERE Id = @ID

        DELETE [dbo].[tbl_UserFolder]
        WHERE Id = @ID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END