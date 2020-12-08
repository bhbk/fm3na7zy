
CREATE PROCEDURE [svc].[usp_Ambassador_Delete]
    @Id UNIQUEIDENTIFIER

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_Ambassador] WHERE Id = @Id

        DELETE [dbo].[tbl_Ambassador]
        WHERE Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END