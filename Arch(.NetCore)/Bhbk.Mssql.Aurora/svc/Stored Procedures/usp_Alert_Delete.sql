



CREATE   PROCEDURE [svc].[usp_Alert_Delete]
    @Id uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_Alert] WHERE Id = @Id

        DELETE [dbo].[tbl_Alert]
        WHERE Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END