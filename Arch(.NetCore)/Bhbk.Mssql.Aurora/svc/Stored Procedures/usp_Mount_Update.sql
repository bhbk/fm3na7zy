
CREATE PROCEDURE [svc].[usp_Mount_Update]
     @UserId		    	UNIQUEIDENTIFIER
    ,@AmbassadorId			UNIQUEIDENTIFIER
    ,@AuthType				NVARCHAR (16) 
    ,@ServerAddress			NVARCHAR (260) 
    ,@ServerShare			NVARCHAR (260) 
    ,@IsEnabled				BIT
	,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @LASTUPDATED DATETIMEOFFSET (7) = GETUTCDATE()

        UPDATE [dbo].[tbl_Mount]
        SET
			 AuthType				= @AuthType
			,ServerAddress			= @ServerAddress
			,ServerShare			= @ServerShare
			,IsEnabled				= @IsEnabled
			,IsDeletable			= @IsDeletable
            ,LastUpdatedUtc			= @LASTUPDATED
        WHERE UserId = @UserId AND AmbassadorId = @AmbassadorId

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_Mount] 
			WHERE UserId = @UserId AND AmbassadorId = @AmbassadorId

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END