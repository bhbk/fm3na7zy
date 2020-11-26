
CREATE PROCEDURE [svc].[usp_UserMount_Insert]
     @IdentityId			UNIQUEIDENTIFIER
    ,@CredentialId			UNIQUEIDENTIFIER
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

		INSERT INTO [dbo].[tbl_UserMount]
			(
			IdentityId
			,CredentialId
			,AuthType  
			,ServerAddress
			,ServerShare
			,IsEnabled
			,IsDeletable
			,CreatedUtc
			)
		VALUES
			(
			@IdentityId
			,@CredentialId
			,@AuthType
			,@ServerAddress
			,@ServerShare
			,@IsEnabled
			,@IsDeletable
			,@CREATEDUTC
			);

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

		SELECT * FROM [dbo].[tbl_UserMount] WHERE IdentityId = @IdentityId

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END