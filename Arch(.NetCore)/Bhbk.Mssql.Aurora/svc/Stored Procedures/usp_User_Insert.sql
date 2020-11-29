
CREATE PROCEDURE [svc].[usp_User_Insert]
     @IdentityId			UNIQUEIDENTIFIER
    ,@IdentityAlias			NVARCHAR (128) 
    ,@FileSystemType		NVARCHAR (16) 
    ,@IsPasswordRequired	BIT
    ,@IsPublicKeyRequired	BIT
    ,@IsFileSystemReadOnly	BIT
    ,@Debugger				NVARCHAR (16) 
    ,@IsEnabled				BIT
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

		INSERT INTO [dbo].[tbl_User]
			(
			 IdentityId
			,IdentityAlias
			,FileSystemType
			,IsPasswordRequired
			,IsPublicKeyRequired
			,IsFileSystemReadOnly
			,QuotaInBytes
			,QuotaUsedInBytes
			,ConcurrentSessions
			,Debugger
			,IsEnabled
			,IsDeletable
			,CreatedUtc
			)
		VALUES
			(
			@IdentityId
			,@IdentityAlias
			,@FileSystemType
			,@IsPasswordRequired
			,@IsPublicKeyRequired
			,@IsFileSystemReadOnly
			,2147483647
			,0
			,1
			,@Debugger
			,@IsEnabled
			,@IsDeletable
			,@CREATEDUTC
			);

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

		SELECT * FROM [dbo].[tbl_User] WHERE IdentityId = @IdentityId

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END