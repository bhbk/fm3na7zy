
CREATE PROCEDURE [svc].[usp_PublicKey_Update]
     @Id					UNIQUEIDENTIFIER 
    ,@UserId		    	UNIQUEIDENTIFIER
    ,@PrivateKeyId			UNIQUEIDENTIFIER
    ,@KeyValue				NVARCHAR (MAX) 
    ,@KeyAlgo				NVARCHAR (16) 
    ,@KeyFormat				NVARCHAR (16) 
    ,@SigValue				NVARCHAR (512) 
    ,@SigAlgo				NVARCHAR (16) 
    ,@Comment				NVARCHAR (1024) 
    ,@IsEnabled				BIT
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @LASTUPDATED DATETIMEOFFSET (7) = GETUTCDATE()

        UPDATE [dbo].[tbl_PublicKey]
        SET
			 PrivateKeyId			= @PrivateKeyId
			,KeyValue				= @KeyValue
			,KeyAlgo				= @KeyAlgo
			,KeyFormat				= @KeyFormat
			,SigValue				= @SigValue
			,SigAlgo				= @SigAlgo
			,Comment				= @Comment
			,IsEnabled				= @IsEnabled
			,IsDeletable			= @IsDeletable
            ,LastUpdatedUtc			= @LASTUPDATED
        WHERE Id = @Id AND UserId = @UserId

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_PublicKey] 
			WHERE Id = @Id AND UserId = @UserId

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END