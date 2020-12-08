
CREATE PROCEDURE [svc].[usp_Mount_Insert]
     @UserId				UNIQUEIDENTIFIER
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

        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

		INSERT INTO [dbo].[tbl_Mount]
			(
			UserId
			,AmbassadorId
			,AuthType  
			,ServerAddress
			,ServerShare
			,IsEnabled
			,IsDeletable
			,CreatedUtc
			)
		VALUES
			(
			@UserId
			,@AmbassadorId
			,@AuthType
			,@ServerAddress
			,@ServerShare
			,@IsEnabled
			,@IsDeletable
			,@CREATEDUTC
			);

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

		SELECT * FROM [dbo].[tbl_Mount] WHERE UserId = @UserId

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END