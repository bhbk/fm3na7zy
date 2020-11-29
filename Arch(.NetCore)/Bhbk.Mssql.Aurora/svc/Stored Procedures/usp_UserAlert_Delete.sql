
CREATE   PROCEDURE [svc].[usp_UserAlert_Delete]
    @ID uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_UserAlert] WHERE Id = @ID

        DELETE [dbo].[tbl_UserAlert]
        WHERE Id = @ID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END