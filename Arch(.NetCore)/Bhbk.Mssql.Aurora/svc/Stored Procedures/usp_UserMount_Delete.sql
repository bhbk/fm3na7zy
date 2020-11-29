
CREATE   PROCEDURE [svc].[usp_UserMount_Delete]
    @IdentityID uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_UserMount] WHERE IdentityId = @IdentityID

        DELETE [dbo].[tbl_UserMount]
        WHERE IdentityId = @IdentityID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END