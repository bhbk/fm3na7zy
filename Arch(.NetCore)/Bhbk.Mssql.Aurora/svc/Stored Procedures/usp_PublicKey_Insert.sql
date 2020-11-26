



CREATE PROCEDURE [svc].[usp_PublicKey_Insert]
     @IdentityId			UNIQUEIDENTIFIER
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

		DECLARE @PUBLICKEYID UNIQUEIDENTIFIER = NEWID()
        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

		INSERT INTO [dbo].[tbl_PublicKey]
			(
			 Id         
			,IdentityId
			,PrivateKeyId   
			,KeyValue
			,KeyAlgo
			,KeyFormat
			,SigValue
			,SigAlgo
			,Comment
			,IsEnabled
			,IsDeletable
			,CreatedUtc
			)
		VALUES
			(
			 @PUBLICKEYID          
			,@IdentityId
			,@PrivateKeyId
			,@KeyValue
			,@KeyAlgo
			,@KeyFormat
			,@SigValue
			,@SigAlgo
			,@Comment
			,@IsEnabled
			,@IsDeletable
			,@CREATEDUTC
			);

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

		SELECT * FROM [dbo].[tbl_PublicKey] WHERE Id = @PUBLICKEYID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END