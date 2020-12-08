
CREATE PROCEDURE [svc].[usp_Ambassador_Update]
     @Id					UNIQUEIDENTIFIER 
    ,@UserName				NVARCHAR (128) 
    ,@EncryptedPass 		NVARCHAR (128) 
    ,@IsEnabled				BIT
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @LASTUPDATED DATETIMEOFFSET (7) = GETUTCDATE()

        UPDATE [dbo].[tbl_Ambassador]
        SET
             Id						= @Id
			,UserName				= @UserName
			,EncryptedPass  		= @EncryptedPass
			,IsEnabled				= @IsEnabled
            ,IsDeletable			= @IsDeletable
            ,LastUpdatedUtc			= @LASTUPDATED
        WHERE Id = @Id

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_Ambassador] WHERE Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END