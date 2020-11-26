
CREATE   PROCEDURE [svc].[usp_Network_Delete]
    @ID uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_Network] WHERE Id = @ID

        DELETE [dbo].[tbl_Network]
        WHERE Id = @ID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END