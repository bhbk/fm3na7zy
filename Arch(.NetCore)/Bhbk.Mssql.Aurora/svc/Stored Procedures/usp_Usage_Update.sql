
CREATE PROCEDURE [svc].[usp_Usage_Update]
     @UserId	    		UNIQUEIDENTIFIER
	,@QuotaInBytes			BIGINT
	,@QuotaUsedInBytes		BIGINT
    ,@SessionMax            SMALLINT
    ,@SessionsInUse         SMALLINT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @LASTUPDATED DATETIMEOFFSET (7) = GETUTCDATE()

        UPDATE [dbo].[tbl_Usage]
        SET
			QuotaInBytes			= @QuotaInBytes
			,QuotaUsedInBytes		= @QuotaUsedInBytes
            ,SessionMax             = @SessionMax
            ,SessionsInUse          = @SessionsInUse
        WHERE UserId = @UserId

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_Usage] 
            WHERE UserId = @UserId

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
