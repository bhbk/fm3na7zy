
CREATE PROCEDURE [svc].[usp_Mount_Delete]
    @UserId UNIQUEIDENTIFIER

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_Mount] WHERE UserId = @UserId

        DELETE [dbo].[tbl_Mount]
        WHERE UserId = @UserId

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END