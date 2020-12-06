
CREATE PROCEDURE [svc].[usp_UserFolder_Delete]
    @Id UNIQUEIDENTIFIER

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_UserFolder] WHERE Id = @Id

        DELETE [dbo].[tbl_UserFolder]
        WHERE Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END