
CREATE PROCEDURE [svc].[usp_Ambassador_Insert]
    @UserName				NVARCHAR (128) 
    ,@EncryptedPass			NVARCHAR (128) 
    ,@IsEnabled				BIT
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

		DECLARE @CREDENTIALID UNIQUEIDENTIFIER = NEWID()
        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

		INSERT INTO [dbo].[tbl_Ambassador]
			(
			 Id         
			,UserName
			,EncryptedPass
			,IsEnabled
			,IsDeletable
			,CreatedUtc
			)
		VALUES
			(
			 @CREDENTIALID          
			,@UserName
			,@EncryptedPass
			,@IsEnabled
			,@IsDeletable
			,@CREATEDUTC
			);

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

		SELECT * FROM [dbo].[tbl_Ambassador] WHERE Id = @CREDENTIALID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END