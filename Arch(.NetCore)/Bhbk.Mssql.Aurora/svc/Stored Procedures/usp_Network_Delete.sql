
CREATE PROCEDURE [svc].[usp_Network_Delete]
    @Id UNIQUEIDENTIFIER

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_Network] WHERE Id = @Id

        DELETE [dbo].[tbl_Network]
        WHERE Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END