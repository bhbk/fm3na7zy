
CREATE PROCEDURE [svc].[usp_UserLogin_Insert]
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

		INSERT INTO [dbo].[tbl_UserLogin]
			(
			 IdentityId
			,IdentityAlias
			,FileSystemType
			,IsPasswordRequired
			,IsPublicKeyRequired
			,IsFileSystemReadOnly
			,QuotaInBytes
			,QuotaUsedInBytes
			,SessionMax
			,SessionsInUse
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
			,1073741824
			,0
			,1
			,0
			,@Debugger
			,@IsEnabled
			,@IsDeletable
			,@CREATEDUTC
			);

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

		SELECT * FROM [dbo].[tbl_UserLogin] WHERE IdentityId = @IdentityId

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END