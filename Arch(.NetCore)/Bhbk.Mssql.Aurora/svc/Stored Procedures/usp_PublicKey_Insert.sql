
CREATE PROCEDURE [svc].[usp_PublicKey_Insert]
	 @Id					UNIQUEIDENTIFIER
    ,@UserId				UNIQUEIDENTIFIER
    ,@PrivateKeyId			UNIQUEIDENTIFIER
    ,@KeyValue				NVARCHAR (MAX) 
    ,@KeyAlgorithmId				INT
    ,@KeyFormatId				INT 
    ,@SigValue				NVARCHAR (512) 
    ,@SigAlgo				INT 
    ,@Comment				NVARCHAR (1024) 
    ,@IsEnabled				BIT
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

    	BEGIN TRANSACTION;

        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

		INSERT INTO [dbo].[tbl_PublicKey]
			(
			 Id         
			,UserId
			,PrivateKeyId   
			,KeyValue
			,KeyAlgorithmId
			,KeyFormatId
			,SigValue
			,SigAlgorithmId
			,Comment
			,IsEnabled
			,IsDeletable
			,CreatedUtc
			)
		VALUES
			(
			 @Id         
			,@UserId
			,@PrivateKeyId
			,@KeyValue
			,@KeyAlgorithmId
			,@KeyFormatId
			,@SigValue
			,@SigAlgo
			,@Comment
			,@IsEnabled
			,@IsDeletable
			,@CREATEDUTC
			);

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

		SELECT * FROM [dbo].[tbl_PublicKey] 
			WHERE Id = @Id

    	COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

    	ROLLBACK TRANSACTION;
        THROW;

    END CATCH

END
