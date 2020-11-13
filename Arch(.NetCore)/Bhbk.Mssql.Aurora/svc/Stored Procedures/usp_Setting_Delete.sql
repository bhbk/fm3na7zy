

CREATE PROCEDURE [svc].[usp_Setting_Delete]
    @ID uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [svc].[uvw_Setting] WHERE [svc].[uvw_Setting].Id = @ID

        DELETE [dbo].[tbl_Setting]
        WHERE Id = @ID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END