
CREATE PROCEDURE [svc].[usp_Session_Delete]
	@Id UNIQUEIDENTIFIER

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_Session] 
            WHERE Id = @Id

		DELETE FROM [dbo].[tbl_Session]
			WHERE Id = @Id;

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
