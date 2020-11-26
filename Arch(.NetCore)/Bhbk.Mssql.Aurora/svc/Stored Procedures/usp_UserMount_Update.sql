





CREATE PROCEDURE [svc].[usp_UserMount_Update]
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

        DECLARE @LASTUPDATED DATETIMEOFFSET (7) = GETUTCDATE()

        UPDATE [dbo].[tbl_UserMount]
        SET
			 AuthType				= @AuthType
			,ServerAddress			= @ServerAddress
			,ServerShare			= @ServerShare
			,IsEnabled				= @IsEnabled
			,IsDeletable			= @IsDeletable
            ,LastUpdatedUtc			= @LASTUPDATED
        WHERE IdentityId = @IdentityId AND CredentialId = @CredentialId

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_UserMount] 
			WHERE IdentityId = @IdentityId AND CredentialId = @CredentialId

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END