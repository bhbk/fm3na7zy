
CREATE   PROCEDURE [svc].[usp_File_Delete]
    @Id uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_File] 
            WHERE Id = @Id

        DELETE [dbo].[tbl_File]
            WHERE Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END