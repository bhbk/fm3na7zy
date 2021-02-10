
CREATE PROCEDURE [svc].[usp_Login_Update]
     @UserId	    		UNIQUEIDENTIFIER
    ,@UserName	    		NVARCHAR (128) 
    ,@AuthTypeId			INT 
    ,@FileSystemTypeId		INT 
	,@FileSystemChrootPath	NVARCHAR (64)
    ,@IsPasswordRequired	BIT
    ,@IsPublicKeyRequired	BIT
    ,@IsFileSystemReadOnly	BIT
    ,@DebugTypeId				INT 
    ,@EncryptedPass			NVARCHAR (1024) 
    ,@IsEnabled				BIT
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

    	BEGIN TRANSACTION;

        UPDATE [dbo].[tbl_Login]
        SET
			UserName		    	= @UserName
            ,AuthTypeId            = @AuthTypeId
			,FileSystemTypeId			= @FileSystemTypeId
			,FileSystemChrootPath	= @FileSystemChrootPath
			,IsPasswordRequired		= @IsPasswordRequired
			,IsPublicKeyRequired	= @IsPublicKeyRequired
			,IsFileSystemReadOnly	= @IsFileSystemReadOnly
			,DebugTypeId				= @DebugTypeId
            ,EncryptedPass          = @EncryptedPass
			,IsEnabled				= @IsEnabled
			,IsDeletable			= @IsDeletable
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