


CREATE PROCEDURE [svc].[usp_Credential_Insert]
    @Domain					NVARCHAR (128) 
    ,@UserName				NVARCHAR (128) 
    ,@EncryptedPassword		NVARCHAR (128) 
    ,@IsEnabled				BIT
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

		DECLARE @CREDENTIALID UNIQUEIDENTIFIER = NEWID()
        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

		INSERT INTO [dbo].[tbl_Credential]
			(
			 Id         
			,Domain   
			,UserName
			,EncryptedPassword
			,IsEnabled
			,IsDeletable
			,CreatedUtc
			)
		VALUES
			(
			 @CREDENTIALID          
			,@Domain
			,@UserName
			,@EncryptedPassword
			,@IsEnabled
			,@IsDeletable
			,@CREATEDUTC
			);

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

		SELECT * FROM [dbo].[tbl_Credential] WHERE Id = @CREDENTIALID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END