
CREATE PROCEDURE [svc].[usp_UserFile_Delete]
    @Id UNIQUEIDENTIFIER

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_UserFile] WHERE Id = @Id

        DELETE [dbo].[tbl_UserFile]
        WHERE Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END