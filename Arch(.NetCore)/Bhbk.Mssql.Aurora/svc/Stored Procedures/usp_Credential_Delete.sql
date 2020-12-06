
CREATE PROCEDURE [svc].[usp_Credential_Delete]
    @Id UNIQUEIDENTIFIER

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_Credential] WHERE Id = @Id

        DELETE [dbo].[tbl_Credential]
        WHERE Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END