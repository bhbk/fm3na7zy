
CREATE PROCEDURE [svc].[usp_PrivateKey_Insert]
     @IdentityId			UNIQUEIDENTIFIER
    ,@PublicKeyId			UNIQUEIDENTIFIER
    ,@KeyValue				NVARCHAR (MAX) 
    ,@KeyAlgo				NVARCHAR (16) 
    ,@KeyPass				NVARCHAR (1024) 
    ,@KeyFormat				NVARCHAR (16) 
    ,@IsEnabled				BIT
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

		DECLARE @PRIVATEKEYID UNIQUEIDENTIFIER = NEWID()
        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

		INSERT INTO [dbo].[tbl_PrivateKey]
			(
			 Id         
			,IdentityId
			,PublicKeyId   
			,KeyValue
			,KeyAlgo
			,KeyPass
			,KeyFormat
			,IsEnabled
			,IsDeletable
			,CreatedUtc
			)
		VALUES
			(
			 @PRIVATEKEYID          
			,@IdentityId
			,@PublicKeyId
			,@KeyValue
			,@KeyAlgo
			,@KeyPass
			,@KeyFormat
			,@IsEnabled
			,@IsDeletable
			,@CREATEDUTC
			);

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

		SELECT * FROM [dbo].[tbl_PrivateKey] WHERE Id = @PRIVATEKEYID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END