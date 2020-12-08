
CREATE PROCEDURE [svc].[usp_Login_Update]
     @UserId	    		UNIQUEIDENTIFIER
    ,@UserName	    		NVARCHAR (128) 
    ,@FileSystemType		NVARCHAR (16) 
	,@FileSystemChrootPath	NVARCHAR (64)
    ,@IsPasswordRequired	BIT
    ,@IsPublicKeyRequired	BIT
    ,@IsFileSystemReadOnly	BIT
    ,@Debugger				NVARCHAR (16) 
    ,@EncryptedPass			NVARCHAR (1024) 
    ,@IsEnabled				BIT
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @LASTUPDATED DATETIMEOFFSET (7) = GETUTCDATE()

        UPDATE [dbo].[tbl_Login]
        SET
			 UserName		    	= @UserName
			,FileSystemType			= @FileSystemType
			,FileSystemChrootPath	= @FileSystemChrootPath
			,IsPasswordRequired		= @IsPasswordRequired
			,IsPublicKeyRequired	= @IsPublicKeyRequired
			,IsFileSystemReadOnly	= @IsFileSystemReadOnly
			,Debugger				= @Debugger
            ,EncryptedPass          = @EncryptedPass
			,IsEnabled				= @IsEnabled
			,IsDeletable			= @IsDeletable
            ,LastUpdatedUtc			= @LASTUPDATED
        WHERE UserId = @UserId

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_Login] WHERE UserId = @UserId

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END