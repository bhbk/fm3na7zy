
CREATE PROCEDURE [svc].[usp_Activity_Delete]
    @ID uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [svc].[uvw_Activity] WHERE [svc].[uvw_Activity].IdentityId = @ID

        DELETE [dbo].[tbl_Activity]
        WHERE IdentityId = @ID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END