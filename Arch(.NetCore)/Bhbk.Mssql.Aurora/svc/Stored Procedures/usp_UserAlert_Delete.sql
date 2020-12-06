
CREATE PROCEDURE [svc].[usp_UserAlert_Delete]
    @Id UNIQUEIDENTIFIER

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_UserAlert] WHERE Id = @Id

        DELETE [dbo].[tbl_UserAlert]
        WHERE Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END