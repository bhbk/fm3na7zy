
CREATE PROCEDURE [svc].[usp_Mount_Update]
     @UserId		    	UNIQUEIDENTIFIER
    ,@AmbassadorId			UNIQUEIDENTIFIER
    ,@AuthType				NVARCHAR (16) 
    ,@UncPath		    	NVARCHAR (260) 
    ,@IsEnabled				BIT
	,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

    	BEGIN TRANSACTION;

        DECLARE @LASTUPDATED DATETIMEOFFSET (7) = GETUTCDATE()

        UPDATE [dbo].[tbl_Mount]
        SET
			 AuthType				= @AuthType
            ,AmbassadorId           = @AmbassadorId
			,UncPath		    	= @UncPath
			,IsEnabled				= @IsEnabled
			,IsDeletable			= @IsDeletable
            ,LastUpdatedUtc			= @LASTUPDATED
        WHERE UserId = @UserId

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_Mount] 
			WHERE UserId = @UserId

    	COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

    	ROLLBACK TRANSACTION;
        THROW;

    END CATCH

END