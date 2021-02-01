﻿
CREATE PROCEDURE [svc].[usp_Login_Update]
     @UserId	    		UNIQUEIDENTIFIER
    ,@UserAuthType			NVARCHAR (16) 
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

    	BEGIN TRANSACTION;

        DECLARE @LASTUPDATED DATETIMEOFFSET (7) = GETUTCDATE()

        UPDATE [dbo].[tbl_Login]
        SET
            UserAuthType            = @UserAuthType
			,UserName		    	= @UserName
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

        SELECT * FROM [dbo].[tbl_Login] 
            WHERE UserId = @UserId

    	COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

    	ROLLBACK TRANSACTION;
        THROW;

    END CATCH

END