
CREATE PROCEDURE [svc].[usp_Credential_Update]
     @Id					UNIQUEIDENTIFIER 
    ,@Domain				NVARCHAR (128) 
    ,@UserName				NVARCHAR (128) 
    ,@EncryptedPassword		NVARCHAR (128) 
    ,@IsEnabled				BIT
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @LASTUPDATED DATETIMEOFFSET (7) = GETUTCDATE()

        UPDATE [dbo].[tbl_Credential]
        SET
             Id						= @Id
			,Domain					= @Domain
			,UserName				= @UserName
			,EncryptedPassword		= @EncryptedPassword
			,IsEnabled				= @IsEnabled
            ,IsDeletable			= @IsDeletable
            ,LastUpdatedUtc			= @LASTUPDATED
        WHERE Id = @Id

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_Credential] WHERE Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END