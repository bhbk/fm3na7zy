
CREATE PROCEDURE [svc].[usp_Setting_Delete]
    @Id UNIQUEIDENTIFIER

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [svc].[uvw_Setting] WHERE [svc].[uvw_Setting].Id = @Id

        DELETE [dbo].[tbl_Setting]
        WHERE Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END