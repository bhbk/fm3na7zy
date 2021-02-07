
CREATE PROCEDURE [svc].[usp_Login_Insert]
     @UserId				UNIQUEIDENTIFIER
    ,@UserAuthType			NVARCHAR (16) 
    ,@UserName				NVARCHAR (128) 
    ,@FileSystemType		NVARCHAR (16) 
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

        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

		INSERT INTO [dbo].[tbl_Login]
			(
			 UserId
			,UserAuthType
			,UserName
			,FileSystemType
			,IsPasswordRequired
			,IsPublicKeyRequired
			,IsFileSystemReadOnly
			,Debugger
			,EncryptedPass
			,IsEnabled
			,IsDeletable
			,CreatedUtc
			)
		VALUES
			(
			@UserId
			,@UserAuthType
			,@UserName
			,@FileSystemType
			,@IsPasswordRequired
			,@IsPublicKeyRequired
			,@IsFileSystemReadOnly
			,@Debugger
			,@EncryptedPass
			,@IsEnabled
			,@IsDeletable
			,@CREATEDUTC
			);

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

		INSERT INTO [dbo].[tbl_LoginUsage]
			(
			 UserId
			,QuotaInBytes
			,QuotaUsedInBytes
			,SessionMax
			,SessionsInUse
			)
		VALUES
			(
			@UserId
			,1073741824
			,0
			,1
			,0
			);

		SELECT * FROM [dbo].[tbl_Login] 
			WHERE UserId = @UserId

    	COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

    	ROLLBACK TRANSACTION;
        THROW;

    END CATCH

END
