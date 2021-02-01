
CREATE PROCEDURE [svc].[usp_Alert_Update]
	 @Id					UNIQUEIDENTIFIER
    ,@OnDelete				BIT
    ,@OnDownload			BIT
    ,@OnUpload				BIT
    ,@ToDisplayName			NVARCHAR (256) 
    ,@ToEmailAddress		NVARCHAR (320) 
    ,@ToPhoneNumber			NVARCHAR (15) 
    ,@IsEnabled				BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

    	BEGIN TRANSACTION;

        DECLARE @LASTUPDATED DATETIMEOFFSET (7) = GETUTCDATE()

        UPDATE [dbo].[tbl_Alert]
        SET
			 OnDelete				= @OnDelete
			,OnDownload				= @OnDownload
			,OnUpload				= @OnUpload
			,ToDisplayName			= @ToDisplayName
			,ToEmailAddress			= @ToEmailAddress
			,ToPhoneNumber			= @ToPhoneNumber
			,IsEnabled				= @IsEnabled
            ,LastUpdatedUtc			= @LASTUPDATED
        WHERE Id = @Id

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_Alert]
			WHERE Id = @Id

    	COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

    	ROLLBACK TRANSACTION;
        THROW;

    END CATCH

END
