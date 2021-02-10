
CREATE PROCEDURE [svc].[usp_PublicKey_Update]
     @Id					UNIQUEIDENTIFIER 
    ,@PrivateKeyId			UNIQUEIDENTIFIER
    ,@KeyValue				NVARCHAR (MAX) 
    ,@KeyAlgorithmId				INT
    ,@KeyFormatId				INT 
    ,@SigValue				NVARCHAR (512) 
    ,@SigAlgorithmId				INT 
    ,@Comment				NVARCHAR (1024) 
    ,@IsEnabled				BIT
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

    	BEGIN TRANSACTION;

        UPDATE [dbo].[tbl_PublicKey]
        SET
			 PrivateKeyId			= @PrivateKeyId
			,KeyValue				= @KeyValue
			,KeyAlgorithmId				= @KeyAlgorithmId
			,KeyFormatId				= @KeyFormatId
			,SigValue				= @SigValue
			,SigAlgorithmId				= @SigAlgorithmId
			,Comment				= @Comment
			,IsEnabled				= @IsEnabled
			,IsDeletable			= @IsDeletable
        WHERE Id = @Id

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