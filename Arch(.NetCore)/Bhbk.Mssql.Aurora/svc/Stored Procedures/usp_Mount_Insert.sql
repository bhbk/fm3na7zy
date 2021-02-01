
CREATE PROCEDURE [svc].[usp_Mount_Insert]
     @UserId				UNIQUEIDENTIFIER
    ,@AmbassadorId			UNIQUEIDENTIFIER
    ,@AuthType				NVARCHAR (16) 
    ,@UncPath				NVARCHAR (260) 
    ,@IsEnabled				BIT
	,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

    	BEGIN TRANSACTION;

        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

		INSERT INTO [dbo].[tbl_Mount]
			(
			UserId
			,AmbassadorId
			,AuthType  
			,UncPath
			,IsEnabled
			,IsDeletable
			,CreatedUtc
			)
		VALUES
			(
			@UserId
			,@AmbassadorId
			,@AuthType
			,@UncPath
			,@IsEnabled
			,@IsDeletable
			,@CREATEDUTC
			);

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
