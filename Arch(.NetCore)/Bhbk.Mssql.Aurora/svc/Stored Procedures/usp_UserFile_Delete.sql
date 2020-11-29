
CREATE   PROCEDURE [svc].[usp_UserFile_Delete]
    @ID uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_UserFile] WHERE Id = @ID

        DELETE [dbo].[tbl_UserFile]
        WHERE Id = @ID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END